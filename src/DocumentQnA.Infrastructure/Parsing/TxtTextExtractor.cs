namespace DocumentQnA.Infrastructure.Parsing;

public class TxtTextExtractor
{
    public async Task<string> ExtractAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}