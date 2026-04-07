using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Contracts.Responses;
using DocumentQnA.Domain.Entities;
using DocumentQnA.Domain.Enums;
using MediatR;

namespace DocumentQnA.Application.QnA.Commands;

public class AskQuestionHandler : IRequestHandler<AskQuestionCommand, AskQuestionResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly ILlmService _llmService;

    private const string CollectionName = "document_chunks";

    public AskQuestionHandler(
        IDocumentRepository documentRepository,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        ILlmService llmService)
    {
        _documentRepository = documentRepository;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _llmService = llmService;
    }

    public async Task<AskQuestionResponse> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate document exists and belongs to user
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.UserId != request.UserId)
            throw new KeyNotFoundException("Document not found.");

        if (document.Status != DocumentStatus.Ready)
            throw new InvalidOperationException("Document is not ready for querying yet.");

        // 2. Embed the question
        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Question, cancellationToken);

        // 3. Search vector store for relevant chunks
        var searchResults = await _vectorStoreService.SearchAsync(CollectionName, questionEmbedding, topK: 5, cancellationToken);

        // 4. Filter results to only chunks belonging to this document
        var relevantResults = searchResults
            .Where(r => r.Payload.ContainsKey("documentId") && r.Payload["documentId"] == request.DocumentId.ToString())
            .ToList();

        var contextChunks = relevantResults.Select(r => r.Payload["content"]).ToList();

        // 5. Ask the LLM with context
        var (answer, tokensUsed) = await _llmService.AskAsync(request.Question, contextChunks, cancellationToken);

        // 6. Build response
        return new AskQuestionResponse
        {
            Question = request.Question,
            Answer = answer,
            DocumentId = request.DocumentId,
            TokensUsed = tokensUsed,
            RelevantChunks = relevantResults.Select(r => new DocumentChunkResponse
            {
                Content = r.Payload["content"],
            }).ToList()
        };
    }
}