using System.Diagnostics;
using System.Security.Claims;
using backend.Data;
using backend.Models;

namespace backend.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var startedAtUtc = DateTimeOffset.UtcNow;
        var timestamp = Stopwatch.GetTimestamp();
        var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
        var failed = false;
        try
        {
            await next(context);
        }
        catch
        {
            failed = true;
            throw;
        }
        finally
        {
            var duration = Stopwatch.GetElapsedTime(timestamp);
            var statusCode = failed && !context.Response.HasStarted
                ? StatusCodes.Status500InternalServerError
                : context.Response.StatusCode;

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.HttpRequestLogs.Add(new HttpRequestLog
                {
                    StartedAtUtc = startedAtUtc,
                    Method = Limit(context.Request.Method, 32)!,
                    RouteTemplate = Limit(route, 512),
                    StatusCode = statusCode,
                    DurationMilliseconds = duration.TotalMilliseconds,
                    TraceId = Limit(Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier, 128)!,
                    UserId = context.User.Identity?.IsAuthenticated == true
                        ? Limit(context.User.FindFirstValue(ClaimTypes.NameIdentifier), 450)
                        : null
                });

                // Give logging its own deadline even if the client disconnected.
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await db.SaveChangesAsync(timeout.Token);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not save HTTP request log");
            }
        }
    }

    private static string? Limit(string? value, int length) =>
        value is not null && value.Length > length ? value[..length] : value;
}
