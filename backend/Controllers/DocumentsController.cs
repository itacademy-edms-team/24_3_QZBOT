using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using backend.Contracts;
using backend.Data;
using backend.Models;
using backend.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController(ApplicationDbContext db, IFileStorageService storage) : ControllerBase
{
    private const int MaxFileBytes = 10 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaxFileBytes)
            return BadRequest(new { error = "Select a non-empty PDF or DOCX file up to 10 MB." });

        var originalName = file.FileName.Replace('\\', '/').Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255)
            return BadRequest(new { error = "Invalid file name." });

        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (extension is not (".pdf" or ".docx"))
            return BadRequest(new { error = "Only PDF and DOCX files are supported." });

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != file.Length || buffer.Length > MaxFileBytes || !HasExpectedFormat(buffer, extension))
            return BadRequest(new { error = "File content does not match its PDF or DOCX extension." });

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var document = new Document
        {
            OwnerId = ownerId,
            OriginalFileName = originalName,
            ContentType = extension == ".pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            SizeBytes = buffer.Length,
            Sha256 = Convert.ToHexString(SHA256.HashData(buffer.GetBuffer().AsSpan(0, (int)buffer.Length)))
                .ToLowerInvariant(),
            StorageKey = string.Empty
        };

        buffer.Position = 0;
        document.StorageKey = await storage.SaveAsync(buffer, cancellationToken);
        try
        {
            db.Documents.Add(document);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(document.StorageKey, CancellationToken.None);
            throw;
        }

        return StatusCode(StatusCodes.Status201Created, new DocumentResponse
        {
            Id = document.Id,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            Sha256 = document.Sha256,
            UploadedAt = document.UploadedAt
        });
    }

    private static bool HasExpectedFormat(MemoryStream buffer, string extension)
    {
        var bytes = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
        if (extension == ".pdf")
            return bytes.StartsWith("%PDF-"u8);

        if (!bytes.StartsWith("PK\x03\x04"u8)) return false;
        try
        {
            buffer.Position = 0;
            using var archive = new ZipArchive(buffer, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count > 1000) return false;
            return archive.GetEntry("[Content_Types].xml") is not null &&
                   archive.GetEntry("word/document.xml") is not null;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}
