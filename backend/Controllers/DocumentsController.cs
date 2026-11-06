using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using backend.Contracts;
using backend.Data;
using backend.Models;
using backend.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController(
    ApplicationDbContext db, IFileStorageService storage, ILogger<DocumentsController> logger) : ControllerBase
{
    private const int MaxDocumentBytes = 10 * 1024 * 1024;
    private const int MaxAudioBytes = 25 * 1024 * 1024;
    private const int MaxArchiveEntries = 20;
    private const long MaxExpandedBytes = 100L * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<List<DocumentResponse>>> List(CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        return await db.Documents.AsNoTracking()
            .Where(document => document.OwnerId == ownerId)
            .OrderByDescending(document => document.UploadedAt)
            .Select(document => new DocumentResponse
            {
                Id = document.Id,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                SizeBytes = document.SizeBytes,
                Sha256 = document.Sha256,
                UploadedAt = document.UploadedAt
            })
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var document = await db.Documents.AsNoTracking()
            .Where(item => item.Id == id && item.OwnerId == ownerId)
            .Select(item => new DocumentResponse
            {
                Id = item.Id,
                OriginalFileName = item.OriginalFileName,
                ContentType = item.ContentType,
                SizeBytes = item.SizeBytes,
                Sha256 = item.Sha256,
                UploadedAt = item.UploadedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return document is null ? NotFound() : document;
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var document = await db.Documents.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (document is null) return NotFound();

        try
        {
            var stream = storage.OpenRead(document.StorageKey);
            return File(stream, document.ContentType, document.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            logger.LogError("Stored file missing for document {DocumentId}", id);
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var document = await db.Documents
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (document is null) return NotFound();

        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
        try
        {
            await storage.DeleteAsync(document.StorageKey, CancellationToken.None);
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not remove stored file for document {DocumentId}", id);
        }

        return NoContent();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(27 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            return BadRequest(new { error = "Select a non-empty PDF, DOCX, MP3, WAV or M4A file." });

        var originalName = file.FileName.Replace('\\', '/').Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255)
            return BadRequest(new { error = "Invalid file name." });

        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (extension is not (".pdf" or ".docx" or ".mp3" or ".wav" or ".m4a"))
            return BadRequest(new { error = "Only PDF, DOCX, MP3, WAV and M4A files are supported." });

        var maxFileBytes = extension is ".pdf" or ".docx" ? MaxDocumentBytes : MaxAudioBytes;
        if (file.Length > maxFileBytes)
            return BadRequest(new { error = $"File exceeds the {maxFileBytes / 1024 / 1024} MB limit." });

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != file.Length || buffer.Length > maxFileBytes || !HasExpectedFormat(buffer, extension))
            return BadRequest(new { error = "File content does not match its extension." });

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        return StatusCode(StatusCodes.Status201Created,
            await SaveDocumentAsync(ownerId, originalName, extension, buffer, cancellationToken));
    }

    [HttpPost("archive")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(27 * 1024 * 1024)]
    public async Task<IActionResult> UploadArchive([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaxAudioBytes ||
            !string.Equals(Path.GetExtension(file.FileName), ".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Select a ZIP archive up to 25 MB." });

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        await using var source = file.OpenReadStream();
        using var archiveBuffer = new MemoryStream((int)file.Length);
        await source.CopyToAsync(archiveBuffer, cancellationToken);
        if (archiveBuffer.Length != file.Length || archiveBuffer.Length > MaxAudioBytes)
            return BadRequest(new { error = "Invalid archive size." });

        try
        {
            archiveBuffer.Position = 0;
            using var archive = new ZipArchive(archiveBuffer, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count == 0 || archive.Entries.Count > MaxArchiveEntries)
                return BadRequest(new { error = "ZIP must contain 1 to 20 entries." });

            var result = new ArchiveUploadResponse();
            long expandedBytes = 0;
            foreach (var entry in archive.Entries)
            {
                var name = entry.FullName;
                if (name.Length > 255 || name.Contains('/') || name.Contains('\\') ||
                    name.Contains(':') || name is "." or ".." || string.IsNullOrWhiteSpace(name))
                {
                    result.Rejected.Add(new RejectedArchiveFile { FileName = name, Reason = "Nested or unsafe path." });
                    continue;
                }

                var extension = Path.GetExtension(name).ToLowerInvariant();
                if (extension is not (".pdf" or ".docx"))
                {
                    result.Rejected.Add(new RejectedArchiveFile { FileName = name, Reason = "Only PDF and DOCX are allowed in ZIP." });
                    continue;
                }

                if (entry.Length is <= 0 or > MaxDocumentBytes ||
                    expandedBytes + entry.Length > MaxExpandedBytes)
                {
                    result.Rejected.Add(new RejectedArchiveFile { FileName = name, Reason = "Expanded size limit exceeded." });
                    continue;
                }

                try
                {
                    using var content = new MemoryStream((int)entry.Length);
                    await using var entryStream = entry.Open();
                    var chunk = new byte[81920];
                    int read;
                    while ((read = await entryStream.ReadAsync(chunk, cancellationToken)) > 0)
                    {
                        if (content.Length + read > MaxDocumentBytes ||
                            expandedBytes + content.Length + read > MaxExpandedBytes)
                            throw new InvalidDataException("Expanded size limit exceeded.");
                        await content.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
                    }

                    if (content.Length != entry.Length || !HasExpectedFormat(content, extension))
                        throw new InvalidDataException("File content does not match its extension.");

                    expandedBytes += content.Length;
                    result.Uploaded.Add(await SaveDocumentAsync(ownerId, name, extension, content, cancellationToken));
                }
                catch (InvalidDataException exception)
                {
                    result.Rejected.Add(new RejectedArchiveFile { FileName = name, Reason = exception.Message });
                }
            }

            return Ok(result);
        }
        catch (InvalidDataException)
        {
            return BadRequest(new { error = "Invalid ZIP archive." });
        }
    }

    private async Task<DocumentResponse> SaveDocumentAsync(
        string ownerId, string originalName, string extension, MemoryStream buffer,
        CancellationToken cancellationToken)
    {
        var document = new Document
        {
            OwnerId = ownerId,
            OriginalFileName = originalName,
            ContentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                _ => "audio/mp4"
            },
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

        return new DocumentResponse
        {
            Id = document.Id,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            Sha256 = document.Sha256,
            UploadedAt = document.UploadedAt
        };
    }

    private static bool HasExpectedFormat(MemoryStream buffer, string extension)
    {
        var bytes = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
        if (extension == ".pdf")
            return bytes.StartsWith("%PDF-"u8);

        if (extension == ".mp3")
            return bytes.Length >= 10 && bytes.StartsWith("ID3"u8) ||
                   bytes.Length >= 4 && bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0;

        if (extension == ".wav")
            return bytes.Length >= 44 && bytes.StartsWith("RIFF"u8) &&
                   bytes[8..].StartsWith("WAVE"u8);

        if (extension == ".m4a")
            return bytes.Length >= 12 && bytes[4..].StartsWith("ftyp"u8) &&
                   (bytes[8..].StartsWith("M4A "u8) || bytes[8..].StartsWith("isom"u8) ||
                    bytes[8..].StartsWith("mp41"u8) || bytes[8..].StartsWith("mp42"u8));

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
