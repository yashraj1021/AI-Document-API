using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.Documents.Queries;

public class GetAllDocumentsQuery : IRequest<List<DocumentResponse>>
{
    public string UserId { get; init; } = string.Empty;

    public GetAllDocumentsQuery(string userId)
    {
        UserId = userId;
    }
}