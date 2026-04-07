namespace DocumentQnA.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default);
}