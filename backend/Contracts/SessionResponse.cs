namespace backend.Contracts;

public sealed class SessionResponse
{
    public bool Authenticated { get; init; }

    public SessionUserResponse? User { get; init; }
}

public sealed class SessionUserResponse
{
    public required string Id { get; init; }

    public required string Email { get; init; }
}
