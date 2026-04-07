using UglyToad.PdfPig;

namespace DocumentQnA.Infrastructure.Parsing;

public class PdfTextExtractor
{
    public string Extract(Stream fileStream)
    {
        using var pdf = PdfDocument.Open(fileStream);
        var text = string.Join(" ", pdf.GetPages().Select(p => p.Text));
        return text;
    }
}