using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.QnA.Commands;

public class AskQuestionCommand : IRequest<AskQuestionResponse>
{
    public Guid DocumentId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Question { get; init; } = string.Empty;

    public AskQuestionCommand(Guid documentId, string userId, string question)
    {
        DocumentId = documentId;
        UserId = userId;
        Question = question;
    }
}