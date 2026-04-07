namespace DocumentQnA.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;  // raw text of this chunk
    public int ChunkIndex { get; set; }                  // order of chunk in document
    public string VectorId { get; set; } = string.Empty; // ID stored in Qdrant

    // Navigation property
    public Document Document { get; set; } = null!;
}