namespace backend.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["FileStorage:RootPath"];
        var rootPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "JusticeSecondOpinion", "files")
            : configuredPath;

        if (!Path.IsPathFullyQualified(rootPath))
            throw new InvalidOperationException("FileStorage:RootPath must be an absolute path.");

        _rootPath = Path.GetFullPath(rootPath);
        var contentRoot = Path.GetFullPath(environment.ContentRootPath);
        if (IsInside(_rootPath, contentRoot))
            throw new InvalidOperationException("File storage must be outside the application directory.");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var key = Guid.NewGuid().ToString("N");
        var temporaryPath = Path.Combine(_rootPath, $"{key}.tmp");
        var finalPath = Path.Combine(_rootPath, key);

        try
        {
            await using (var destination = new FileStream(temporaryPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await source.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, finalPath);
            return key;
        }
        catch
        {
            File.Delete(temporaryPath);
            throw;
        }
    }

    public Stream OpenRead(string storageKey)
    {
        return new FileStream(GetFilePath(storageKey), FileMode.Open, FileAccess.Read,
            FileShare.Read, 81920, useAsync: true);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(GetFilePath(storageKey));
        return Task.CompletedTask;
    }

    private string GetFilePath(string storageKey)
    {
        if (!Guid.TryParseExact(storageKey, "N", out var parsed) ||
            storageKey != parsed.ToString("N"))
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));

        return Path.Combine(_rootPath, storageKey);
    }

    private static bool IsInside(string path, string parent)
    {
        var relative = Path.GetRelativePath(parent, path);
        return relative == "." ||
            (!Path.IsPathRooted(relative) && relative != ".." &&
             !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }
}
