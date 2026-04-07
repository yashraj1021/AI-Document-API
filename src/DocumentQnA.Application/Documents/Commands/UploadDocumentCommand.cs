using DocumentQnA.Contracts.Responses;
using MediatR;

namespace DocumentQnA.Application.Documents.Commands;

public class UploadDocumentCommand : IRequest<DocumentResponse>
{
    public string UserId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Stream FileStream { get; init; } = Stream.Null;

    public UploadDocumentCommand(string userId, string fileName, string fileType, long fileSizeBytes, Stream fileStream)
    {
        UserId = userId;
        FileName = fileName;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
        FileStream = fileStream;
    }
}