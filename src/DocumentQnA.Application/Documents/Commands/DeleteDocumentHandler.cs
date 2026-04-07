using DocumentQnA.Application.Common.Interfaces;
using MediatR;

namespace DocumentQnA.Application.Documents.Commands;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DeleteDocumentHandler(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.UserId != request.UserId)
            return false;

        await _fileStorageService.DeleteAsync(document.StoragePath, cancellationToken);
        await _documentRepository.DeleteAsync(request.DocumentId, cancellationToken);

        return true;
    }
}