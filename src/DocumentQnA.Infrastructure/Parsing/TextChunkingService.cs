namespace DocumentQnA.Infrastructure.Parsing;

public class TextChunkingService
{
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;

    public TextChunkingService(int chunkSize = 500, int chunkOverlap = 50)
    {
        _chunkSize = chunkSize;
        _chunkOverlap = chunkOverlap;
    }

    public List<string> Chunk(string text)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int index = 0;
        while (index < words.Length)
        {
            var chunk = string.Join(" ", words.Skip(index).Take(_chunkSize));
            chunks.Add(chunk);
            index += _chunkSize - _chunkOverlap;
        }

        return chunks;
    }
}