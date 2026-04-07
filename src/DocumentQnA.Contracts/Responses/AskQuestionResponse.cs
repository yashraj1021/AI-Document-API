namespace DocumentQnA.Contracts.Responses;

public class AskQuestionResponse
{
    public string Answer { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }
    public List<DocumentChunkResponse> RelevantChunks { get; set; } = new(); // chunks used as context
    public int TokensUsed { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}