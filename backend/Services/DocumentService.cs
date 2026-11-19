using System.Security.Cryptography;
using backend.Contracts;
using backend.Data;
using backend.Models;
using backend.Storage;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public enum DeleteDocumentResult { Deleted, NotFound, StorageError }

public sealed class DocumentService(
    ApplicationDbContext db, IFileStorageService storage, DocumentFormatValidator formats,
    ILogger<DocumentService> logger)
{
    public Task<List<DocumentResponse>> ListAsync(string ownerId, CancellationToken cancellationToken) =>
        db.Documents.AsNoTracking()
            .Where(document => document.OwnerId == ownerId)
            .OrderByDescending(document => document.UploadedAt)
            .Select(document => new DocumentResponse
            {
                Id = document.Id,
                Note = document.Note ?? (document.ArchiveUpload == null ? null : document.ArchiveUpload.Note),
                ArchiveUploadId = document.ArchiveUploadId,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                SizeBytes = document.SizeBytes,
                Sha256 = document.Sha256,
                UploadedAt = document.UploadedAt
            })
            .ToListAsync(cancellationToken);

    public Task<DocumentResponse?> GetAsync(string ownerId, Guid id, CancellationToken cancellationToken) =>
        db.Documents.AsNoTracking()
            .Where(document => document.Id == id && document.OwnerId == ownerId)
            .Select(document => new DocumentResponse
            {
                Id = document.Id,
                Note = document.Note ?? (document.ArchiveUpload == null ? null : document.ArchiveUpload.Note),
                ArchiveUploadId = document.ArchiveUploadId,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                SizeBytes = document.SizeBytes,
                Sha256 = document.Sha256,
                UploadedAt = document.UploadedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<(Stream Stream, string ContentType, string FileName)?> OpenDownloadAsync(
        string ownerId, Guid id, CancellationToken cancellationToken)
    {
        var document = await db.Documents.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (document is null) return null;
        try
        {
            return (storage.OpenRead(document.StorageKey), document.ContentType, document.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            logger.LogError("Stored file missing for document {DocumentId}", id);
            return null;
        }
    }

    public async Task<DeleteDocumentResult> DeleteAsync(string ownerId, Guid id, CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (document is null) return DeleteDocumentResult.NotFound;
        try
        {
            // Keep the row when file deletion fails; a retry can finish the operation.
            // Missing files are tolerated by the storage implementation.
            await storage.DeleteAsync(document.StorageKey, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogError(exception, "Could not remove stored file for document {DocumentId}", id);
            return DeleteDocumentResult.StorageError;
        }
        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
        return DeleteDocumentResult.Deleted;
    }

    public async Task<DocumentResponse> SaveAsync(
        string ownerId, string originalName, string extension, MemoryStream buffer,
        CancellationToken cancellationToken, string? note = null, ArchiveUpload? archiveUpload = null)
    {
        var document = new Document
        {
            OwnerId = ownerId,
            Note = note,
            ArchiveUpload = archiveUpload,
            OriginalFileName = originalName,
            ContentType = formats.ContentType(extension),
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
            Note = document.Note ?? archiveUpload?.Note,
            ArchiveUploadId = document.ArchiveUploadId,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            Sha256 = document.Sha256,
            UploadedAt = document.UploadedAt
        };
    }
}
