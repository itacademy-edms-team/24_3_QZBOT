using System.Security.Claims;
using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/cases")]
public sealed class CourtCasesController(ApplicationDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CourtCaseResponse>> Create(
        CreateCourtCaseRequest request, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var courtCase = new CourtCase { OwnerId = ownerId, Title = request.Title.Trim() };
        db.CourtCases.Add(courtCase);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CourtCaseResponse(courtCase.Id, courtCase.Title, courtCase.CreatedAt);
        return CreatedAtAction(nameof(Get), new { id = courtCase.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<List<CourtCaseResponse>>> List(CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        return await OwnedCases(ownerId)
            .OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Select(item => new CourtCaseResponse(item.Id, item.Title, item.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourtCaseResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var courtCase = await OwnedCases(ownerId)
            .Where(item => item.Id == id)
            .Select(item => new CourtCaseResponse(item.Id, item.Title, item.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return courtCase is null ? NotFound() : courtCase;
    }

    private IQueryable<CourtCase> OwnedCases(string ownerId) =>
        db.CourtCases.AsNoTracking().Where(item => item.OwnerId == ownerId);
}
