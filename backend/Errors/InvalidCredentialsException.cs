namespace backend.Errors;

public sealed class InvalidCredentialsException()
    : Exception("Invalid email or password.");
