namespace Insight.WebApi.Models;

public class BlogEntryResponse
{
    public string Content { get; set; } = string.Empty;
    public List<string> Citations { get; set; } = new();
    public List<string> References { get; set; } = new();
    public decimal QualityScore { get; set; }
    public List<string> QualityIssues { get; set; } = new();
    public List<string> QualityStrengths { get; set; } = new();
}
