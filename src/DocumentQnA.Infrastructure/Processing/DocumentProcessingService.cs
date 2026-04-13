using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Domain.Entities;
using DocumentQnA.Domain.Enums;
using DocumentQnA.Infrastructure.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DocumentQnA.Infrastructure.Persistence;

namespace DocumentQnA.Infrastructure.Processing;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly TextChunkingService _chunkingService;
    private readonly ILogger<DocumentProcessingService> _logger;

    private const string CollectionName = "document_chunks";
    private const int VectorSize = 768;

    public DocumentProcessingService(
        IDbContextFactory<AppDbContext> contextFactory,
        IFileStorageService fileStorageService,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        TextChunkingService chunkingService,
        ILogger<DocumentProcessingService> logger)
    {
        _contextFactory = contextFactory;
        _fileStorageService = fileStorageService;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _chunkingService = chunkingService;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting processing for document {DocumentId}", documentId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var document = await context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken); // removed Include

        if (document is null)
        {
            _logger.LogWarning("Document {DocumentId} not found", documentId);
            return;
        }

        try
        {
            document.Status = DocumentStatus.Processing;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Document {DocumentId} status set to Processing", documentId);

            var fileStream = await _fileStorageService.GetAsync(document.StoragePath, cancellationToken);
            var rawText = await ExtractTextAsync(document.FileType, fileStream);
            _logger.LogInformation("Extracted {Length} characters of text", rawText.Length);

            var chunks = _chunkingService.Chunk(rawText);
            _logger.LogInformation("Created {ChunkCount} chunks", chunks.Count);

            await _vectorStoreService.CreateCollectionIfNotExistsAsync(CollectionName, VectorSize, cancellationToken);

            var chunksToAdd = new List<DocumentChunk>();

            for (int i = 0; i < chunks.Count; i++)
            {
                _logger.LogInformation("Embedding chunk {Index} of {Total}", i + 1, chunks.Count);
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i], cancellationToken);
                _logger.LogInformation("Chunk {Index} embedded, vector size: {Size}", i + 1, embedding.Length);

                var vectorId = Guid.NewGuid();
                var payload = new Dictionary<string, string>
            {
                { "documentId", document.Id.ToString() },
                { "content", chunks[i] },
                { "chunkIndex", i.ToString() }
            };

                await _vectorStoreService.UpsertAsync(CollectionName, vectorId, embedding, payload, cancellationToken);

                chunksToAdd.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    VectorId = vectorId.ToString()
                });
            }

            // Insert chunks as new rows
            await context.DocumentChunks.AddRangeAsync(chunksToAdd, cancellationToken);

            // Update document status
            document.Status = DocumentStatus.Ready;
            document.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Document {DocumentId} processing complete!", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);
            document.Status = DocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string> ExtractTextAsync(string fileType, Stream fileStream)
    {
        return fileType.ToLower() switch
        {
            "pdf" => new PdfTextExtractor().Extract(fileStream),
            "docx" => new DocxTextExtractor().Extract(fileStream),
            "txt" => await new TxtTextExtractor().ExtractAsync(fileStream),
            _ => throw new NotSupportedException($"File type '{fileType}' is not supported.")
        };
    }
}