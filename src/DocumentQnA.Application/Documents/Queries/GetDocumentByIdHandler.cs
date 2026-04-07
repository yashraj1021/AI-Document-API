using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.Documents.Queries;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentResponse?>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentResponse?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.UserId != request.UserId)
            return null;

        return new DocumentResponse
        {
            Id = document.Id,
            FileName = document.FileName,
            FileType = document.FileType,
            FileSizeBytes = document.FileSizeBytes,
            Status = document.Status.ToString(),
            ErrorMessage = document.ErrorMessage,
            CreatedAt = document.CreatedAt,
            ProcessedAt = document.ProcessedAt
        };
    }
}