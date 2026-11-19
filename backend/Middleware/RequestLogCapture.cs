using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace Backend.Middleware;

internal sealed class RequestLogCapture
{
    private const int MaxJsonBytes = 4096;
    private static readonly string[] KnownFields = ["email", "password", "title", "note", "text"];
    private readonly string bodyMetadata;
    private readonly Stream? bufferedBody;
    private readonly int jsonLength;

    public RequestLogCapture(HttpRequest request)
    {
        var mediaType = MediaTypeHeaderValue.TryParse(request.ContentType, out var parsed)
            ? parsed.MediaType.Value?.ToLowerInvariant() switch
            {
                "application/json" => "application/json",
                "multipart/form-data" => "multipart/form-data",
                "application/x-www-form-urlencoded" => "application/x-www-form-urlencoded",
                null => null,
                _ => "other"
            }
            : null;

        // Preserve only safe header values. Even custom header names can carry sensitive data.
        Headers = JsonSerializer.Serialize(new
        {
            contentType = mediaType,
            contentLength = request.ContentLength,
            omittedCount = request.Headers.Keys.Count(name =>
                !name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
        });

        var canHaveBody = request.HttpContext.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody
            ?? request.ContentLength != 0;
        bodyMetadata = JsonSerializer.Serialize(new
        {
            kind = canHaveBody ? "omitted" : "empty",
            contentType = mediaType,
            length = request.ContentLength
        });

        if (mediaType == "application/json" && request.ContentLength is > 0 and <= MaxJsonBytes)
        {
            request.EnableBuffering(bufferThreshold: MaxJsonBytes);
            bufferedBody = request.Body;
            jsonLength = (int)request.ContentLength.Value;
        }
    }

    public string Headers { get; }

    public async Task<string> DescribeBodyAsync()
    {
        // Only inspect bytes already consumed by the application. Do not read an unread upload.
        if (bufferedBody is not { CanSeek: true } || bufferedBody.Length < jsonLength)
            return bodyMetadata;

        var position = bufferedBody.Position;
        try
        {
            bufferedBody.Position = 0;
            var bytes = new byte[jsonLength];
            await bufferedBody.ReadExactlyAsync(bytes);
            using var json = JsonDocument.Parse(bytes);
            var root = json.RootElement;
            return JsonSerializer.Serialize(new
            {
                kind = root.ValueKind.ToString(),
                length = jsonLength,
                fields = root.ValueKind == JsonValueKind.Object
                    ? KnownFields.Where(name => root.TryGetProperty(name, out _)).ToArray()
                    : []
            });
        }
        catch (Exception exception) when (exception is IOException or JsonException or ObjectDisposedException)
        {
            return bodyMetadata;
        }
        finally
        {
            if (bufferedBody.CanSeek) bufferedBody.Position = position;
        }
    }
}
