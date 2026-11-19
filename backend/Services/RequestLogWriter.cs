using Backend.Data;

namespace Backend.Services;

public sealed class RequestLogWriter(
    RequestLogQueue queue, IServiceScopeFactory scopeFactory, ILogger<RequestLogWriter> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.HttpRequestLogs.Add(entry);

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));
                await db.SaveChangesAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not save HTTP request log {TraceId}", entry.TraceId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Complete();
        try
        {
            if (ExecuteTask is { } worker)
                await worker.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Shutdown deadline reached while writing queued HTTP request logs");
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
