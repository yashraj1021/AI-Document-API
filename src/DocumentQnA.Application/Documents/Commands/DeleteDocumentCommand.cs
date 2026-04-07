using MediatR;

namespace DocumentQnA.Application.Documents.Commands;

public class DeleteDocumentCommand : IRequest<bool>
{
    public Guid DocumentId { get; init; }
    public string UserId { get; init; } = string.Empty;

    public DeleteDocumentCommand(Guid documentId, string userId)
    {
        DocumentId = documentId;
        UserId = userId;
    }
}