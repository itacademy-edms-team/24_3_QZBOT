using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
