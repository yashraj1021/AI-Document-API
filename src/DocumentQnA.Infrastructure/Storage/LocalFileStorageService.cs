using DocumentQnA.Application.Common.Interfaces;

namespace DocumentQnA.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);

        await using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output, cancellationToken);

        return filePath;
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(storagePath))
            File.Delete(storagePath);

        return Task.CompletedTask;
    }

    public Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
            throw new FileNotFoundException("File not found.", storagePath);

        Stream stream = File.OpenRead(storagePath);
        return Task.FromResult(stream);
    }
}