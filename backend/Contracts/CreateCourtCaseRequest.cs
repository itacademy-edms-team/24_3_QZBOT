using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public sealed record CreateCourtCaseRequest([Required, StringLength(255)] string Title);
