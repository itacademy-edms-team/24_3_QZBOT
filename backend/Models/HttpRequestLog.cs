namespace backend.Models;

public sealed class HttpRequestLog
{
    public long Id { get; set; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public required string Method { get; init; }
    public string? RouteTemplate { get; init; }
    public int StatusCode { get; init; }
    public double DurationMilliseconds { get; init; }
    public required string TraceId { get; init; }
    public string? UserId { get; init; }
}
