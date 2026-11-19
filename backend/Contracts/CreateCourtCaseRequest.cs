using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts;

public sealed record CreateCourtCaseRequest([Required, StringLength(255)] string Title);
