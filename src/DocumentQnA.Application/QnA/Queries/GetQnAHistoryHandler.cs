using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.QnA.Queries;

public class GetQnAHistoryHandler : IRequestHandler<GetQnAHistoryQuery, List<QnAHistoryResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetQnAHistoryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<List<QnAHistoryResponse>> Handle(GetQnAHistoryQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.UserId != request.UserId)
            return new List<QnAHistoryResponse>();

        return document.QnAHistories.Select(h => new QnAHistoryResponse
        {
            Id = h.Id,
            DocumentId = h.DocumentId,
            Question = h.Question,
            Answer = h.Answer,
            TokensUsed = h.TokensUsed,
            AskedAt = h.AskedAt
        }).ToList();
    }
}