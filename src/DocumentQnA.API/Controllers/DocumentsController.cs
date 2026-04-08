using DocumentQnA.Application.Documents.Commands;
using DocumentQnA.Application.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentQnA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var allowedTypes = new[] { "pdf", "docx", "txt" };
        var fileType = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

        if (!allowedTypes.Contains(fileType))
            return BadRequest(new { error = $"File type '{fileType}' is not supported. Allowed: pdf, docx, txt" });

        var command = new UploadDocumentCommand(
            UserId,
            file.FileName,
            fileType,
            file.Length,
            file.OpenReadStream()
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllDocumentsQuery(UserId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentByIdQuery(id, UserId);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteDocumentCommand(id, UserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result ? NoContent() : NotFound();
    }
}