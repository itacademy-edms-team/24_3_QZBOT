namespace Backend.Contracts;

public sealed record CourtCaseResponse(Guid Id, string Title, DateTimeOffset CreatedAt);
