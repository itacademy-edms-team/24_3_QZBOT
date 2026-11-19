using System.Diagnostics;
using System.Security.Claims;
using Backend.Models;
using Backend.Services;

namespace Backend.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next, RequestLogQueue queue, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;
        var timestamp = Stopwatch.GetTimestamp();
        var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
        var capture = new RequestLogCapture(context.Request);
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

            var entry = new HttpRequestLog
            {
                StartedAtUtc = startedAtUtc,
                Method = Limit(context.Request.Method, 32)!,
                RouteTemplate = Limit(route, 512),
                StatusCode = statusCode,
                DurationMilliseconds = duration.TotalMilliseconds,
                TraceId = Limit(Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier, 128)!,
                RequestHeaders = capture.Headers,
                RequestBody = await capture.DescribeBodyAsync(),
                UserId = context.User.Identity?.IsAuthenticated == true
                    ? Limit(context.User.FindFirstValue(ClaimTypes.NameIdentifier), 450)
                    : null
            };

            if (!queue.TryEnqueue(entry))
                logger.LogWarning("HTTP request log {TraceId} was dropped because the queue could not accept it", entry.TraceId);
        }
    }

    private static string? Limit(string? value, int length) =>
        value is not null && value.Length > length ? value[..length] : value;
}
