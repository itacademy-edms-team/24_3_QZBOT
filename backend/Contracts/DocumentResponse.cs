namespace backend.Contracts;

public sealed class DocumentResponse
{
    public required Guid Id { get; init; }
    public required string OriginalFileName { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required string Sha256 { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
}
