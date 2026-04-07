namespace DocumentQnA.Contracts.Responses;

public class DocumentChunkResponse
{
    public Guid Id { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
}