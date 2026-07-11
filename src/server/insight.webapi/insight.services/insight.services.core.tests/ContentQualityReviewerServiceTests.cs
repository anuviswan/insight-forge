using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;

namespace Insight.Services.Core.Tests;

[TestClass]
public class ContentQualityReviewerServiceTests
{
    private IContentQualityReviewer _reviewer;

    [TestInitialize]
    public void Setup()
    {
        _reviewer = new ContentQualityReviewerService();
    }

    [TestMethod]
    public void ReviewContent_WithEmptyContent_ReturnsZeroScore()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.AreEqual(0, result.QualityScore);
        Assert.IsTrue(result.IssuesFound.Any(i => i.Contains("empty")));
    }

    [TestMethod]
    public void ReviewContent_WithPoorStructure_FlagsIssues()
    {
        // Arrange
        var content = "Just plain text with no structure at all. No headings, no formatting.";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.IssuesFound.Any(i => i.Contains("headings") || i.Contains("structure")));
        Assert.IsTrue(result.QualityScore < 1);
    }

    [TestMethod]
    public void ReviewContent_WithGoodStructure_IdentifiesStrengths()
    {
        // Arrange
        var content = @"
# Main Heading

## Section One

Some paragraph content here.

## Section Two

More content with proper formatting.

### Subsection

Even more details.
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("structure")));
    }

    [TestMethod]
    public void ReviewContent_WithShortContent_FlagsLength()
    {
        // Arrange
        var content = "This is a short blog post that is way too brief.";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.IssuesFound.Any(i => i.Contains("short")));
    }

    [TestMethod]
    public void ReviewContent_WithAppropriateLength_PraisesLength()
    {
        // Arrange
        var words = string.Join(" ", Enumerable.Range(1, 600).Select(i => $"word{i}"));
        var content = $"# Blog Post\n\n{words}";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("length")));
    }

    [TestMethod]
    public void ReviewContent_WithCodeBlocks_IdentifiesCodeExamples()
    {
        // Arrange
        var content = @"
# Programming Blog

Here's some code:

```csharp
var x = 5;
Console.WriteLine(x);
```

More text here.
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("code")));
    }

    [TestMethod]
    public void ReviewContent_WithoutCitations_FlagsIssue()
    {
        // Arrange
        var content = "# Blog Post\n\nSome content without any links or citations.";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.IssuesFound.Any(i => i.Contains("citation") || i.Contains("links")));
    }

    [TestMethod]
    public void ReviewContent_WithSufficientCitations_PraisesCitations()
    {
        // Arrange
        var content = @"
# Blog Post

Here's a [reference](https://example.com) to something.
And another [link](https://another.com) for context.
Plus [one more](https://third.com) for good measure.

## References
- Source A
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("citation")));
    }

    [TestMethod]
    public void ReviewContent_WithReferenceSection_PraisesReferences()
    {
        // Arrange
        var content = @"
# Blog Post

Content here.

## References
- Smith (2023)
- Johnson (2022)
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("References")));
    }

    [TestMethod]
    public void ReviewContent_WithLists_IdentifiesFormatting()
    {
        // Arrange
        var content = @"
# Blog Post

Key points:
- Point one
- Point two
- Point three

Final thoughts.
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.Strengths.Any(s => s.Contains("lists")));
    }

    [TestMethod]
    public void ReviewContent_CompleteQualityBlog_ProducesHighScore()
    {
        // Arrange
        var content = @"
# Understanding Cloud Architecture

Cloud computing has revolutionized how organizations [deploy applications](https://aws.amazon.com).

## Core Concepts

Cloud architecture involves several key principles:
- Scalability across distributed systems
- Redundancy for fault tolerance
- Cost optimization through resource sharing

### Infrastructure Components

```yaml
LoadBalancer:
  - distributes traffic
Database:
  - persistent storage
Cache:
  - performance optimization
```

## Deployment Strategies

You can use [containerization](https://docker.com) for consistent deployments.
Learn more from [Kubernetes documentation](https://kubernetes.io).

## Best Practices

1. Monitor performance metrics
2. Implement auto-scaling policies
3. Use managed services when possible
4. Plan for disaster recovery

## References
- Amazon Web Services (2023)
- Google Cloud Documentation (2022)
- Microsoft Azure Guides (2023)

## Conclusion

Cloud architecture continues to evolve with new tools and practices emerging regularly.
";

        // Act
        var result = _reviewer.ReviewContent(content);

        // Assert
        Assert.IsTrue(result.QualityScore >= 0.7m, $"Expected score >= 0.7, got {result.QualityScore}");
        Assert.IsTrue(result.Strengths.Count > 2);
    }
}
