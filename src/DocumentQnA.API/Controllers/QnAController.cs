using DocumentQnA.Application.QnA.Commands;
using DocumentQnA.Application.QnA.Queries;
using DocumentQnA.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentQnA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QnAController : ControllerBase
{
    private readonly IMediator _mediator;

    public QnAController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var command = new AskQuestionCommand(request.DocumentId, UserId, request.Question);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history/{documentId:guid}")]
    public async Task<IActionResult> GetHistory(Guid documentId, CancellationToken cancellationToken)
    {
        var query = new GetQnAHistoryQuery(documentId, UserId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}