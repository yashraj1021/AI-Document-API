using System.Net.Http.Json;
using DocumentQnA.Application.Common.Interfaces;

namespace DocumentQnA.Infrastructure.AI;

public class QdrantVectorStoreService : IVectorStoreService
{
    private readonly HttpClient _httpClient;

    public QdrantVectorStoreService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/collections/{collectionName}", cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var body = new { vectors = new { size = vectorSize, distance = "Cosine" } };
        var createResponse = await _httpClient.PutAsJsonAsync($"/collections/{collectionName}", body, cancellationToken);
        createResponse.EnsureSuccessStatusCode();
    }

    public async Task UpsertAsync(string collectionName, Guid vectorId, float[] embedding, Dictionary<string, string> payload, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            points = new[]
            {
                new { id = vectorId.ToString(), vector = embedding, payload }
            }
        };

        var response = await _httpClient.PutAsJsonAsync($"/collections/{collectionName}/points", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<(Guid VectorId, double Score, Dictionary<string, string> Payload)>> SearchAsync(string collectionName, float[] queryEmbedding, int topK = 5, CancellationToken cancellationToken = default)
    {
        var body = new { vector = queryEmbedding, limit = topK, with_payload = true };
        var response = await _httpClient.PostAsJsonAsync($"/collections/{collectionName}/points/search", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QdrantSearchResponse>(cancellationToken: cancellationToken);

        return result?.Result?.Select(r => (
            Guid.Parse(r.Id),
            r.Score,
            r.Payload
        )).ToList() ?? [];
    }

    public async Task DeleteAsync(string collectionName, Guid vectorId, CancellationToken cancellationToken = default)
    {
        var body = new { points = new[] { vectorId.ToString() } };
        var response = await _httpClient.PostAsJsonAsync($"/collections/{collectionName}/points/delete", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private class QdrantSearchResponse
    {
        public List<QdrantSearchResult> Result { get; set; } = [];
    }

    private class QdrantSearchResult
    {
        public string Id { get; set; } = string.Empty;
        public double Score { get; set; }
        public Dictionary<string, string> Payload { get; set; } = [];
    }
}