namespace Backend.Models;

public sealed class CourtCase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

    public required string Title { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Document> Documents { get; } = new List<Document>();
}
