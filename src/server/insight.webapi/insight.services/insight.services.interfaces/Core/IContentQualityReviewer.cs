namespace Insight.Services.Interfaces.Core;

public interface IContentQualityReviewer
{
    ContentQualityAssessment ReviewContent(string content);
}

public class ContentQualityAssessment
{
    public decimal QualityScore { get; set; }
    public List<string> IssuesFound { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
}
