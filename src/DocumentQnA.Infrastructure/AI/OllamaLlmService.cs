using System.Net.Http.Json;
using System.Text;
using DocumentQnA.Application.Common.Interfaces;

namespace DocumentQnA.Infrastructure.AI;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaLlmService(HttpClient httpClient, string model = "llama3.2")
    {
        _httpClient = httpClient;
        _model = model;
    }

    public async Task<(string Answer, int TokensUsed)> AskAsync(string question, List<string> contextChunks, CancellationToken cancellationToken = default)
    {
        var context = string.Join("\n\n", contextChunks);

        var prompt = new StringBuilder();
        prompt.AppendLine("You are a helpful assistant. Answer the question using ONLY the context provided below.");
        prompt.AppendLine("If the answer is not in the context, say 'I could not find an answer in the document.'");
        prompt.AppendLine();
        prompt.AppendLine("Context:");
        prompt.AppendLine(context);
        prompt.AppendLine();
        prompt.AppendLine($"Question: {question}");
        prompt.AppendLine("Answer:");

        var request = new { model = _model, prompt = prompt.ToString(), stream = false };
        var response = await _httpClient.PostAsJsonAsync("/api/generate", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);

        return (result?.Response ?? string.Empty, (result?.EvalCount ?? 0) + (result?.PromptEvalCount ?? 0));
    }

    private class OllamaGenerateResponse
    {
        public string Response { get; set; } = string.Empty;
        public int EvalCount { get; set; }          // output tokens
        public int PromptEvalCount { get; set; }    // input tokens
    }
}