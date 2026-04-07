using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.Documents.Queries;

public class GetDocumentByIdQuery : IRequest<DocumentResponse?>
{
    public Guid DocumentId { get; init; }
    public string UserId { get; init; } = string.Empty;

    public GetDocumentByIdQuery(Guid documentId, string userId)
    {
        DocumentId = documentId;
        UserId = userId;
    }
}