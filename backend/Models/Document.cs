namespace backend.Models;

public sealed class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

    public Guid? CaseId { get; set; }

    public required string OriginalFileName { get; set; }

    public required string StorageKey { get; set; }

    public required string ContentType { get; set; }

    public long SizeBytes { get; set; }

    public required string Sha256 { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
