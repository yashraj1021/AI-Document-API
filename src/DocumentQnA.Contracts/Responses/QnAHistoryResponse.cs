namespace DocumentQnA.Contracts.Responses;

public class QnAHistoryResponse
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public DateTime AskedAt { get; set; }
}