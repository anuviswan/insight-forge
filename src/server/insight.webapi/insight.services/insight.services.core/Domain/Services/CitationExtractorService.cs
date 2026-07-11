using System.Text.RegularExpressions;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class CitationExtractorService : ICitationExtractor
{
    public CitationInfo ExtractCitations(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new CitationInfo();

        var info = new CitationInfo();

        info.Citations = ExtractMarkdownLinks(content);
        info.References = ExtractReferenceSection(content);

        return info;
    }

    private static List<string> ExtractMarkdownLinks(string content)
    {
        var links = new List<string>();
        var pattern = @"\[([^\]]+)\]\(([^)]+)\)";
        var matches = Regex.Matches(content, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var linkText = match.Groups[1].Value;
                var url = match.Groups[2].Value;
                links.Add($"{linkText}: {url}");
            }
        }

        return links.Distinct().ToList();
    }

    private static List<string> ExtractReferenceSection(string content)
    {
        var references = new List<string>();

        var refPattern = @"^##\s+References\s*$.*?(?=^##|\Z)";
        var refMatch = Regex.Match(content, refPattern, RegexOptions.Multiline | RegexOptions.Singleline);

        if (refMatch.Success)
        {
            var refSection = refMatch.Value;
            var itemPattern = @"^-\s+(.+)$";
            var items = Regex.Matches(refSection, itemPattern, RegexOptions.Multiline);

            foreach (Match item in items)
            {
                references.Add(item.Groups[1].Value);
            }
        }

        return references;
    }
}
