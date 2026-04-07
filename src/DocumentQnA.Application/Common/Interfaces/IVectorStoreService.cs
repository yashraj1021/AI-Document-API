namespace DocumentQnA.Application.Common.Interfaces;

public interface IVectorStoreService
{
    Task UpsertAsync(string collectionName, Guid vectorId, float[] embedding, Dictionary<string, string> payload, CancellationToken cancellationToken = default);
    Task<List<(Guid VectorId, double Score, Dictionary<string, string> Payload)>> SearchAsync(string collectionName, float[] queryEmbedding, int topK = 5, CancellationToken cancellationToken = default);
    Task DeleteAsync(string collectionName, Guid vectorId, CancellationToken cancellationToken = default);
    Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default);
}