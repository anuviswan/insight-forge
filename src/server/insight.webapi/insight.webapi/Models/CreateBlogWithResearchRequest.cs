namespace Insight.WebApi.Models;

public class CreateBlogWithResearchRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string WritingStyle { get; set; } = string.Empty;
    public string ResearchArtifacts { get; set; } = string.Empty;
}
