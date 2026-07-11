using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using Moq;

namespace Insight.Services.Core.Tests;

[TestClass]
public class BlogServiceTests
{
    private Mock<IBlogAgent> _mockBlogAgent;
    private Mock<ICitationExtractor> _mockCitationExtractor;
    private Mock<IContentQualityReviewer> _mockQualityReviewer;
    private BlogService _blogService;

    [TestInitialize]
    public void Setup()
    {
        _mockBlogAgent = new Mock<IBlogAgent>();
        _mockCitationExtractor = new Mock<ICitationExtractor>();
        _mockQualityReviewer = new Mock<IContentQualityReviewer>();

        _blogService = new BlogService(
            _mockBlogAgent.Object,
            _mockCitationExtractor.Object,
            _mockQualityReviewer.Object
        );
    }

    [TestMethod]
    public async Task CreateBlogEntryAsync_CallsAgentWithCorrectParameters()
    {
        // Arrange
        var topic = "Test Topic";
        var audience = "Developers";
        var style = "Technical";
        var blogContent = "# Blog\n\nContent here.";

        _mockBlogAgent.Setup(x => x.CreateBlogPostAsync(topic, audience, style, default))
            .ReturnsAsync(new BlogEntry { Content = blogContent });

        _mockCitationExtractor.Setup(x => x.ExtractCitations(blogContent))
            .Returns(new CitationInfo { Citations = new(), References = new() });

        _mockQualityReviewer.Setup(x => x.ReviewContent(blogContent))
            .Returns(new ContentQualityAssessment { QualityScore = 0.8m });

        // Act
        var result = await _blogService.CreateBlogEntryAsync(topic, audience, style);

        // Assert
        _mockBlogAgent.Verify(
            x => x.CreateBlogPostAsync(topic, audience, style, default),
            Times.Once
        );
    }

    [TestMethod]
    public async Task CreateBlogEntryAsync_ExtractsCitationsFromContent()
    {
        // Arrange
        var blogContent = "# Blog\n\n[Link](https://example.com)";
        var expectedCitations = new List<string> { "Link: https://example.com" };

        _mockBlogAgent.Setup(x => x.CreateBlogPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(new BlogEntry { Content = blogContent });

        _mockCitationExtractor.Setup(x => x.ExtractCitations(blogContent))
            .Returns(new CitationInfo { Citations = expectedCitations, References = new() });

        _mockQualityReviewer.Setup(x => x.ReviewContent(blogContent))
            .Returns(new ContentQualityAssessment { QualityScore = 0.8m });

        // Act
        var result = await _blogService.CreateBlogEntryAsync("test", "audience", "style");

        // Assert
        Assert.AreEqual(1, result.Citations.Count);
        Assert.AreEqual(expectedCitations[0], result.Citations[0]);
    }

    [TestMethod]
    public async Task CreateBlogEntryAsync_ExtractsReferencesFromContent()
    {
        // Arrange
        var blogContent = "# Blog\n\n## References\n- Ref 1\n- Ref 2";
        var expectedReferences = new List<string> { "Ref 1", "Ref 2" };

        _mockBlogAgent.Setup(x => x.CreateBlogPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(new BlogEntry { Content = blogContent });

        _mockCitationExtractor.Setup(x => x.ExtractCitations(blogContent))
            .Returns(new CitationInfo { Citations = new(), References = expectedReferences });

        _mockQualityReviewer.Setup(x => x.ReviewContent(blogContent))
            .Returns(new ContentQualityAssessment { QualityScore = 0.8m });

        // Act
        var result = await _blogService.CreateBlogEntryAsync("test", "audience", "style");

        // Assert
        Assert.AreEqual(2, result.References.Count);
    }

    [TestMethod]
    public async Task CreateBlogEntryAsync_PerformsQualityReview()
    {
        // Arrange
        var blogContent = "# Blog\n\nContent here.";
        var qualityAssessment = new ContentQualityAssessment
        {
            QualityScore = 0.85m,
            IssuesFound = new() { "Minor issue" },
            Strengths = new() { "Good structure" }
        };

        _mockBlogAgent.Setup(x => x.CreateBlogPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(new BlogEntry { Content = blogContent });

        _mockCitationExtractor.Setup(x => x.ExtractCitations(blogContent))
            .Returns(new CitationInfo());

        _mockQualityReviewer.Setup(x => x.ReviewContent(blogContent))
            .Returns(qualityAssessment);

        // Act
        var result = await _blogService.CreateBlogEntryAsync("test", "audience", "style");

        // Assert
        Assert.IsNotNull(result.QualityAssessment);
        Assert.AreEqual(0.85m, result.QualityAssessment.QualityScore);
        Assert.AreEqual(1, result.QualityAssessment.IssuesFound.Count);
    }

    [TestMethod]
    public async Task CreateBlogEntryAsync_ReturnsBlogEntryWithAllFields()
    {
        // Arrange
        var blogContent = "# Blog\n\n[Link](https://example.com)\n\n## References\n- Ref1";
        var blogEntry = new BlogEntry { Content = blogContent };

        _mockBlogAgent.Setup(x => x.CreateBlogPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(blogEntry);

        _mockCitationExtractor.Setup(x => x.ExtractCitations(blogContent))
            .Returns(new CitationInfo
            {
                Citations = new() { "Link: https://example.com" },
                References = new() { "Ref1" }
            });

        _mockQualityReviewer.Setup(x => x.ReviewContent(blogContent))
            .Returns(new ContentQualityAssessment
            {
                QualityScore = 0.75m,
                IssuesFound = new() { "Could be improved" },
                Strengths = new() { "Well structured" }
            });

        // Act
        var result = await _blogService.CreateBlogEntryAsync("test", "audience", "style");

        // Assert
        Assert.AreEqual(blogContent, result.Content);
        Assert.AreEqual(1, result.Citations.Count);
        Assert.AreEqual(1, result.References.Count);
        Assert.IsNotNull(result.QualityAssessment);
        Assert.AreEqual(0.75m, result.QualityAssessment.QualityScore);
    }
}
