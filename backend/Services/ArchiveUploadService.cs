using System.IO.Compression;
using backend.Contracts;

namespace backend.Services;

public sealed class ArchiveUploadService(DocumentService documents, DocumentFormatValidator formats)
{
    private const int MaxArchiveEntries = 20;
    private const long MaxExpandedBytes = 100L * 1024 * 1024;

    public async Task<(ArchiveUploadResponse? Result, string? Error)> UploadAsync(
        string ownerId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var source = file.OpenReadStream();
        using var archiveBuffer = new MemoryStream((int)file.Length);
        await source.CopyToAsync(archiveBuffer, cancellationToken);
        if (archiveBuffer.Length != file.Length || archiveBuffer.Length > DocumentFormatValidator.MaxAudioBytes)
            return (null, "Invalid archive size.");

        try
        {
            archiveBuffer.Position = 0;
            using var archive = new ZipArchive(archiveBuffer, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count is < 1 or > MaxArchiveEntries)
                return (null, "ZIP must contain 1 to 20 entries.");
            return (await ProcessEntriesAsync(ownerId, archive, cancellationToken), null);
        }
        catch (InvalidDataException)
        {
            return (null, "Invalid ZIP archive.");
        }
    }

    private async Task<ArchiveUploadResponse> ProcessEntriesAsync(
        string ownerId, ZipArchive archive, CancellationToken cancellationToken)
    {
        var result = new ArchiveUploadResponse();
        long expandedBytes = 0;
        foreach (var entry in archive.Entries)
        {
            var rejection = CheckEntry(entry, expandedBytes);
            if (rejection is not null)
            {
                Reject(result, entry.FullName, rejection);
                continue;
            }

            try
            {
                using var content = await ReadEntryAsync(entry, expandedBytes, cancellationToken);
                var extension = Path.GetExtension(entry.FullName).ToLowerInvariant();
                if (content.Length != entry.Length || !formats.HasExpectedFormat(content, extension))
                    throw new InvalidDataException("File content does not match its extension.");
                expandedBytes += content.Length;
                result.Uploaded.Add(await documents.SaveAsync(
                    ownerId, entry.FullName, extension, content, cancellationToken));
            }
            catch (InvalidDataException exception)
            {
                Reject(result, entry.FullName, exception.Message);
            }
        }
        return result;
    }

    private string? CheckEntry(ZipArchiveEntry entry, long expandedBytes)
    {
        var name = entry.FullName;
        if (name.Length > 255 || name.Contains('/') || name.Contains('\\') ||
            name.Contains(':') || name is "." or ".." || string.IsNullOrWhiteSpace(name))
            return "Nested or unsafe path.";
        if (!formats.IsAllowedInArchive(Path.GetExtension(name).ToLowerInvariant()))
            return "Only PDF and DOCX are allowed in ZIP.";
        if (entry.Length is <= 0 or > DocumentFormatValidator.MaxDocumentBytes ||
            expandedBytes + entry.Length > MaxExpandedBytes)
            return "Expanded size limit exceeded.";
        return null;
    }

    private static async Task<MemoryStream> ReadEntryAsync(
        ZipArchiveEntry entry, long expandedBytes, CancellationToken cancellationToken)
    {
        var content = new MemoryStream((int)entry.Length);
        try
        {
            await using var entryStream = entry.Open();
            var chunk = new byte[81920];
            int read;
            while ((read = await entryStream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (content.Length + read > DocumentFormatValidator.MaxDocumentBytes ||
                    expandedBytes + content.Length + read > MaxExpandedBytes)
                    throw new InvalidDataException("Expanded size limit exceeded.");
                await content.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
            }
            return content;
        }
        catch
        {
            content.Dispose();
            throw;
        }
    }

    private static void Reject(ArchiveUploadResponse result, string name, string reason) =>
        result.Rejected.Add(new RejectedArchiveFile { FileName = name, Reason = reason });
}
