namespace backend.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream source, CancellationToken cancellationToken = default);

    Stream OpenRead(string storageKey);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
