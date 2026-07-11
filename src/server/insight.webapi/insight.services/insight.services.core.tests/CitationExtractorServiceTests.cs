using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;

namespace Insight.Services.Core.Tests;

[TestClass]
public class CitationExtractorServiceTests
{
    private ICitationExtractor _extractor;

    [TestInitialize]
    public void Setup()
    {
        _extractor = new CitationExtractorService();
    }

    [TestMethod]
    public void ExtractCitations_WithEmptyContent_ReturnsEmptyCitationInfo()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Citations.Count);
        Assert.AreEqual(0, result.References.Count);
    }

    [TestMethod]
    public void ExtractCitations_WithMarkdownLinks_ExtractsCitations()
    {
        // Arrange
        var content = @"
This is a blog post with [Link Text](https://example.com) and another [More Links](https://another.com).
";

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Citations.Count);
        Assert.IsTrue(result.Citations.Any(c => c.Contains("Link Text")));
        Assert.IsTrue(result.Citations.Any(c => c.Contains("More Links")));
    }

    [TestMethod]
    public void ExtractCitations_WithDuplicateLinks_RemovesDuplicates()
    {
        // Arrange
        var content = @"
Visit [Example](https://example.com) for more info.
Check [Example](https://example.com) again.
";

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.AreEqual(1, result.Citations.Count);
    }

    [TestMethod]
    public void ExtractCitations_WithReferenceSection_ExtracterReferences()
    {
        // Arrange
        var content = @"
# Blog Post

Some content here.

## References
- Smith, J. (2023). Reference One
- Johnson, A. (2022). Reference Two
- Williams, B. (2021). Reference Three

## Next Section
More content.
";

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.References.Count);
        Assert.IsTrue(result.References.Any(r => r.Contains("Smith")));
        Assert.IsTrue(result.References.Any(r => r.Contains("Johnson")));
        Assert.IsTrue(result.References.Any(r => r.Contains("Williams")));
    }

    [TestMethod]
    public void ExtractCitations_WithoutReferenceSection_ReturnsEmptyReferences()
    {
        // Arrange
        var content = @"
# Blog Post
This is a blog without a references section.
";

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.AreEqual(0, result.References.Count);
    }

    [TestMethod]
    public void ExtractCitations_WithCompleteContent_ExtractsAllElements()
    {
        // Arrange
        var content = @"
# Complete Blog Post

This post contains [Link One](https://site1.com) and [Link Two](https://site2.com).

More content here with [Link Three](https://site3.com).

## References
- Source A (2023)
- Source B (2022)

## Conclusion
End of post.
";

        // Act
        var result = _extractor.ExtractCitations(content);

        // Assert
        Assert.AreEqual(3, result.Citations.Count);
        Assert.AreEqual(2, result.References.Count);
    }
}
