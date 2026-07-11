using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class BlogService(IBlogAgent blogAgent, ICitationExtractor citationExtractor, IContentQualityReviewer qualityReviewer) : IBlogService
{
    public async Task<BlogEntry> CreateBlogEntryAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        var blogEntry = await blogAgent.CreateBlogPostAsync(topic, audience, writingStyle, cancellationToken).ConfigureAwait(false);

        blogEntry.Citations = citationExtractor.ExtractCitations(blogEntry.Content).Citations;
        blogEntry.References = citationExtractor.ExtractCitations(blogEntry.Content).References;
        blogEntry.QualityAssessment = qualityReviewer.ReviewContent(blogEntry.Content);

        return blogEntry;
    }
}
