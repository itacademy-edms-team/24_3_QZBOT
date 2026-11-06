using Microsoft.AspNetCore.Identity;

namespace backend.Errors;

public sealed class IdentityValidationException(IEnumerable<IdentityError> errors)
    : Exception("Identity rejected the registration request.")
{
    public IReadOnlyList<IdentityError> Errors { get; } = errors.ToArray();
}
