using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.QnA.Queries;

public class GetQnAHistoryQuery : IRequest<List<QnAHistoryResponse>>
{
    public Guid DocumentId { get; init; }
    public string UserId { get; init; } = string.Empty;

    public GetQnAHistoryQuery(Guid documentId, string userId)
    {
        DocumentId = documentId;
        UserId = userId;
    }
}