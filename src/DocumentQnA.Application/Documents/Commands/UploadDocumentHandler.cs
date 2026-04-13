using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Contracts.Responses;
using DocumentQnA.Domain.Entities;
using DocumentQnA.Domain.Enums;
using MediatR;

namespace DocumentQnA.Application.Documents.Commands;

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentProcessingService _processingService;

    public UploadDocumentHandler(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentProcessingService processingService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _processingService = processingService;
    }

    public async Task<DocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Save the raw file to storage
        var storagePath = await _fileStorageService.SaveAsync(request.FileStream, request.FileName, cancellationToken);

        // 2. Create document record in DB
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FileName = request.FileName,
            FileType = request.FileType,
            FileSizeBytes = request.FileSizeBytes,
            StoragePath = storagePath,
            Status = DocumentStatus.Pending
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        // 3. Trigger processing in background — fire and forget
        // Use Task.Run so it runs AFTER the current request completes
        _ = Task.Run(async () =>
        {
            await _processingService.ProcessAsync(document.Id, CancellationToken.None);
        });

        // 4. Return immediately with Pending status
        return new DocumentResponse
        {
            Id = document.Id,
            FileName = document.FileName,
            FileType = document.FileType,
            FileSizeBytes = document.FileSizeBytes,
            Status = document.Status.ToString(),
            CreatedAt = document.CreatedAt
        };
    }
}