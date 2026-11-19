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

    [HttpGet("{id:guid}/documents")]
    public async Task<ActionResult<List<DocumentResponse>>> ListDocuments(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var documents = await OwnedCases(ownerId)
            .Where(item => item.Id == id)
            .Select(item => item.Documents
                .Where(document => document.OwnerId == ownerId)
                .OrderByDescending(document => document.UploadedAt)
                .ThenBy(document => document.Id)
                .Select(document => new DocumentResponse
                {
                    Id = document.Id,
                    OriginalFileName = document.OriginalFileName,
                    ContentType = document.ContentType,
                    SizeBytes = document.SizeBytes,
                    Sha256 = document.Sha256,
                    UploadedAt = document.UploadedAt
                }).ToList())
            .SingleOrDefaultAsync(cancellationToken);

        return documents is null ? NotFound() : documents;
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> AttachDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var documents = DocumentsForOwnedCase(ownerId, id, documentId);
        var updated = await documents.Where(document => document.CaseId == null || document.CaseId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(document => document.CaseId, (Guid?)id),
                cancellationToken);
        if (updated > 0) return NoContent();

        return await documents.AnyAsync(cancellationToken)
            ? Conflict(new { error = "Document is already attached to another case." })
            : NotFound();
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> DetachDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var updated = await DocumentsForOwnedCase(ownerId, id, documentId)
            .Where(document => document.CaseId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(document => document.CaseId, (Guid?)null),
                cancellationToken);

        return updated > 0 ? NoContent() : NotFound();
    }

    private IQueryable<Document> DocumentsForOwnedCase(string ownerId, Guid caseId, Guid documentId) =>
        db.Documents.Where(document => document.Id == documentId && document.OwnerId == ownerId &&
            db.CourtCases.Any(item => item.Id == caseId && item.OwnerId == ownerId));

    private IQueryable<CourtCase> OwnedCases(string ownerId) =>
        db.CourtCases.AsNoTracking().Where(item => item.OwnerId == ownerId);
}
