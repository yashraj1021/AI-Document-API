using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.Documents.Queries;

public class GetAllDocumentsHandler : IRequestHandler<GetAllDocumentsQuery, List<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetAllDocumentsHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<List<DocumentResponse>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetAllByUserIdAsync(request.UserId, cancellationToken);

        return documents.Select(d => new DocumentResponse
        {
            Id = d.Id,
            FileName = d.FileName,
            FileType = d.FileType,
            FileSizeBytes = d.FileSizeBytes,
            Status = d.Status.ToString(),
            ErrorMessage = d.ErrorMessage,
            CreatedAt = d.CreatedAt,
            ProcessedAt = d.ProcessedAt
        }).ToList();
    }
}