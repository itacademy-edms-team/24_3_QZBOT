namespace backend.Contracts;

public sealed class ArchiveUploadResponse
{
    public List<DocumentResponse> Uploaded { get; init; } = [];
    public List<RejectedArchiveFile> Rejected { get; init; } = [];
}

public sealed class RejectedArchiveFile
{
    public required string FileName { get; init; }
    public required string Reason { get; init; }
}
