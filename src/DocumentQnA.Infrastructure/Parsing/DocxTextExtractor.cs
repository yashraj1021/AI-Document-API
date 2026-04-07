using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocumentQnA.Infrastructure.Parsing;

public class DocxTextExtractor
{
    public string Extract(Stream fileStream)
    {
        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return string.Empty;

        return string.Join(" ", body.Descendants<Text>().Select(t => t.Text));
    }
}