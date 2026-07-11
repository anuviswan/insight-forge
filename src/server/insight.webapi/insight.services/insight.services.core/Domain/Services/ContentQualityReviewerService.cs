using System.Text.RegularExpressions;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class ContentQualityReviewerService : IContentQualityReviewer
{
    public ContentQualityAssessment ReviewContent(string content)
    {
        var assessment = new ContentQualityAssessment { QualityScore = 0 };

        if (string.IsNullOrWhiteSpace(content))
        {
            assessment.QualityScore = 0;
            assessment.IssuesFound.Add("Content is empty");
            return assessment;
        }

        var scoreComponents = new List<decimal>();

        scoreComponents.Add(CheckStructure(content, assessment));
        scoreComponents.Add(CheckLength(content, assessment));
        scoreComponents.Add(CheckFormatting(content, assessment));
        scoreComponents.Add(CheckCitations(content, assessment));
        scoreComponents.Add(CheckHeadings(content, assessment));

        assessment.QualityScore = Math.Round(scoreComponents.Average(), 2);

        return assessment;
    }

    private static decimal CheckStructure(string content, ContentQualityAssessment assessment)
    {
        var score = 1m;

        if (content.Contains("## ") || content.Contains("# "))
            assessment.Strengths.Add("Good document structure with headings");
        else
        {
            assessment.IssuesFound.Add("Missing section headings");
            score -= 0.2m;
        }

        if (content.Contains("\n\n"))
            assessment.Strengths.Add("Good paragraph separation");
        else
        {
            assessment.IssuesFound.Add("Poor paragraph formatting");
            score -= 0.1m;
        }

        return Math.Max(score, 0);
    }

    private static decimal CheckLength(string content, ContentQualityAssessment assessment)
    {
        var wordCount = content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var score = 1m;

        if (wordCount < 300)
        {
            assessment.IssuesFound.Add($"Content too short ({wordCount} words). Aim for 500+ words");
            score -= 0.3m;
        }
        else if (wordCount >= 500 && wordCount <= 3000)
        {
            assessment.Strengths.Add($"Good content length ({wordCount} words)");
        }
        else if (wordCount > 3000)
        {
            assessment.IssuesFound.Add($"Content very long ({wordCount} words). Consider breaking into sections");
            score -= 0.15m;
        }

        return Math.Max(score, 0);
    }

    private static decimal CheckFormatting(string content, ContentQualityAssessment assessment)
    {
        var score = 1m;

        var codeBlockCount = Regex.Matches(content, @"```").Count / 2;
        if (codeBlockCount > 0)
            assessment.Strengths.Add($"Includes {codeBlockCount} code example(s)");

        var listCount = Regex.Matches(content, @"^-\s+|^\*\s+|^\d+\.\s+", RegexOptions.Multiline).Count;
        if (listCount > 0)
            assessment.Strengths.Add("Uses formatted lists");
        else
        {
            assessment.IssuesFound.Add("No formatted lists found");
            score -= 0.1m;
        }

        var linkCount = Regex.Matches(content, @"\[([^\]]+)\]\(([^)]+)\)").Count;
        if (linkCount == 0)
        {
            assessment.IssuesFound.Add("No citations or links found");
            score -= 0.15m;
        }

        return Math.Max(score, 0);
    }

    private static decimal CheckCitations(string content, ContentQualityAssessment assessment)
    {
        var linkCount = Regex.Matches(content, @"\[([^\]]+)\]\(([^)]+)\)").Count;
        var score = 1m;

        if (linkCount >= 3)
        {
            assessment.Strengths.Add($"Strong citations ({linkCount} links)");
        }
        else if (linkCount > 0)
        {
            assessment.IssuesFound.Add($"Limited citations ({linkCount} links). Aim for 3+");
            score -= 0.2m;
        }
        else
        {
            assessment.IssuesFound.Add("No citations found");
            score -= 0.3m;
        }

        if (content.Contains("## References"))
            assessment.Strengths.Add("Includes References section");
        else
        {
            assessment.IssuesFound.Add("Missing References section");
            score -= 0.1m;
        }

        return Math.Max(score, 0);
    }

    private static decimal CheckHeadings(string content, ContentQualityAssessment assessment)
    {
        var headingCount = Regex.Matches(content, @"^#+\s+", RegexOptions.Multiline).Count;
        var score = 1m;

        if (headingCount >= 3)
        {
            assessment.Strengths.Add($"Good heading structure ({headingCount} headings)");
        }
        else if (headingCount > 0)
        {
            assessment.IssuesFound.Add($"Few headings ({headingCount}). Use more for structure");
            score -= 0.15m;
        }
        else
        {
            assessment.IssuesFound.Add("No headings found");
            score -= 0.25m;
        }

        return Math.Max(score, 0);
    }
}
