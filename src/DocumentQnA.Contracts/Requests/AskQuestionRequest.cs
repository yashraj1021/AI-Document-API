namespace DocumentQnA.Contracts.Requests;

public class AskQuestionRequest
{
    public Guid DocumentId { get; set; }
    public string Question { get; set; } = string.Empty;
}