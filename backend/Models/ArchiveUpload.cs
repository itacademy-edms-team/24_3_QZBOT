namespace backend.Models;

public sealed class ArchiveUpload
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Note { get; set; }
}
