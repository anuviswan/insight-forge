namespace Insight.Services.Interfaces.Core;

public interface ICitationExtractor
{
    CitationInfo ExtractCitations(string content);
}

public class CitationInfo
{
    public List<string> Citations { get; set; } = new();
    public List<string> References { get; set; } = new();
}
