using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Domain.Entities;
using DocumentQnA.Domain.Enums;
using DocumentQnA.Infrastructure.Parsing;

namespace DocumentQnA.Infrastructure.Processing;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly TextChunkingService _chunkingService;

    private const string CollectionName = "document_chunks";
    private const int VectorSize = 768; // nomic-embed-text output size

    public DocumentProcessingService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        TextChunkingService chunkingService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _chunkingService = chunkingService;
    }

    public async Task ProcessAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null) return;

        try
        {
            // 1. Update status to Processing
            document.Status = DocumentStatus.Processing;
            await _documentRepository.UpdateAsync(document, cancellationToken);

            // 2. Get the file stream
            var fileStream = await _fileStorageService.GetAsync(document.StoragePath, cancellationToken);

            // 3. Extract text based on file type
            var rawText = await ExtractTextAsync(document.FileType, fileStream);

            // 4. Chunk the text
            var chunks = _chunkingService.Chunk(rawText);

            // 5. Ensure Qdrant collection exists
            await _vectorStoreService.CreateCollectionIfNotExistsAsync(CollectionName, VectorSize, cancellationToken);

            // 6. Embed each chunk and store in Qdrant + DB
            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i], cancellationToken);
                var vectorId = Guid.NewGuid();

                var payload = new Dictionary<string, string>
                {
                    { "documentId", document.Id.ToString() },
                    { "content", chunks[i] },
                    { "chunkIndex", i.ToString() }
                };

                await _vectorStoreService.UpsertAsync(CollectionName, vectorId, embedding, payload, cancellationToken);

                document.Chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    VectorId = vectorId.ToString()
                });
            }

            // 7. Mark as Ready
            document.Status = DocumentStatus.Ready;
            document.ProcessedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }
        catch (Exception ex)
        {
            document.Status = DocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            await _documentRepository.UpdateAsync(document, cancellationToken);
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