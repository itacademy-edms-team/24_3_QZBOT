using System.Security.Claims;
using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController(
    DocumentService documents, DocumentFormatValidator formats, ArchiveUploadService archives) : ControllerBase
{
    private const int MaxRequestBytes = 27 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<List<DocumentResponse>>> List(CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(ownerId) ? Unauthorized() : await documents.ListAsync(ownerId, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
        return await documents.GetAsync(ownerId, id, cancellationToken) is { } document ? document : NotFound();
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
        var download = await documents.OpenDownloadAsync(ownerId, id, cancellationToken);
        return download is null ? NotFound() : File(download.Value.Stream, download.Value.ContentType, download.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
        return await documents.DeleteAsync(ownerId, id, cancellationToken) switch
        {
            DeleteDocumentResult.NotFound => NotFound(),
            DeleteDocumentResult.StorageError => Problem("Could not remove the stored file. Please retry.",
                statusCode: StatusCodes.Status500InternalServerError),
            _ => NoContent()
        };
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            return BadRequest(new { error = "Select a non-empty PDF, DOCX, JPEG, PNG, MP3, WAV or M4A file." });

        var originalName = DocumentFormatValidator.FileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255)
            return BadRequest(new { error = "Invalid file name." });
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (!formats.IsAllowed(extension))
            return BadRequest(new { error = "Only PDF, DOCX, JPEG, PNG, MP3, WAV and M4A files are supported." });
        var maxFileBytes = formats.MaxBytes(extension);
        if (file.Length > maxFileBytes)
            return BadRequest(new { error = $"File exceeds the {maxFileBytes / 1024 / 1024} MB limit." });

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != file.Length || buffer.Length > maxFileBytes || !formats.HasExpectedFormat(buffer, extension))
            return BadRequest(new { error = "File content does not match its extension." });

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
        return StatusCode(StatusCodes.Status201Created,
            await documents.SaveAsync(ownerId, originalName, extension, buffer, cancellationToken));
    }

    [HttpPost("archive")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> UploadArchive([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > DocumentFormatValidator.MaxAudioBytes ||
            !string.Equals(Path.GetExtension(file.FileName), ".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Select a ZIP archive up to 25 MB." });

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();

        var (result, error) = await archives.UploadAsync(ownerId, file, cancellationToken);
        return error is null ? Ok(result) : BadRequest(new { error });
    }
}
