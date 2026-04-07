namespace DocumentQnA.Application.Common.Interfaces;

public interface ILlmService
{
    Task<(string Answer, int TokensUsed)> AskAsync(string question, List<string> contextChunks, CancellationToken cancellationToken = default);
}