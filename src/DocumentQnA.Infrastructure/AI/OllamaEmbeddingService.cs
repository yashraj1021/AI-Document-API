using System.Net.Http.Json;
using DocumentQnA.Application.Common.Interfaces;

namespace DocumentQnA.Infrastructure.AI;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaEmbeddingService(HttpClient httpClient, string model = "nomic-embed-text")
    {
        _httpClient = httpClient;
        _model = model;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new { model = _model, prompt = text };
        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);
        return result?.Embedding ?? [];
    }

    private class OllamaEmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];
    }
}