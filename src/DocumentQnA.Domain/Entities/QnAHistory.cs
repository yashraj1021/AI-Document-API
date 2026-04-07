namespace DocumentQnA.Domain.Entities;

public class QnAHistory
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Document Document { get; set; } = null!;
}