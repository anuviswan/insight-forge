using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Insight.WebApi.Services;

public class BlogService(
    IBlogAgent blogAgent,
    ICitationExtractor citationExtractor,
    IContentQualityReviewer qualityReviewer,
    IJobAgentService jobAgentService,
    IBlogJobResultStore resultStore,
    IServiceScopeFactory scopeFactory,
    ILogger<BlogService> logger) : IBlogService
{
    public async Task<BlogEntry> CreateBlogEntryAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        var blogEntry = await blogAgent.CreateBlogPostAsync(topic, audience, writingStyle, cancellationToken).ConfigureAwait(false);

        blogEntry.Citations = citationExtractor.ExtractCitations(blogEntry.Content).Citations;
        blogEntry.References = citationExtractor.ExtractCitations(blogEntry.Content).References;
        blogEntry.QualityAssessment = qualityReviewer.ReviewContent(blogEntry.Content);

        return blogEntry;
    }

    public Task<string> StartBlogEntryJobAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        var jobId = Guid.NewGuid().ToString();
        var eventBus = jobAgentService.GetOrCreateEventBus(jobId);

        // Run generation in a fresh DI scope so scoped services outlive the HTTP request
        // that kicked off the job (the request completes as soon as jobId is returned).
        _ = Task.Run(() => RunJobAsync(jobId, topic, audience, writingStyle, eventBus));

        return Task.FromResult(jobId);
    }

    public BlogJobResult? GetJobResult(string jobId) => resultStore.GetResult(jobId);

    private async Task RunJobAsync(string jobId, string topic, string audience, string writingStyle, IEventBus eventBus)
    {
        // Correlation scope: flows to every logger invoked while this job runs (Gemini
        // API client, stream wrapper, etc.) so all log lines for a job can be traced together.
        using var correlationScope = logger.BeginScope("JobId:{JobId}", jobId);
        using var scope = scopeFactory.CreateScope();
        var scopedBlogAgent = scope.ServiceProvider.GetRequiredService<IBlogAgent>();
        var scopedCitationExtractor = scope.ServiceProvider.GetRequiredService<ICitationExtractor>();
        var scopedQualityReviewer = scope.ServiceProvider.GetRequiredService<IContentQualityReviewer>();

        try
        {
            var blogEntry = await scopedBlogAgent.CreateBlogPostStreamedAsync(topic, audience, writingStyle, eventBus, CancellationToken.None)
                .ConfigureAwait(false);

            // A stream that completes without ever throwing but produces no text is not
            // a successful generation - without this guard it would be stored via
            // SetResult and reported to the client as a 200 OK success.
            if (string.IsNullOrWhiteSpace(blogEntry.Content))
            {
                const string errorMessage = "Blog generation completed but produced no content";
                logger.LogWarning("Blog generation job {JobId} completed with empty content", jobId);
                resultStore.SetError(jobId, errorMessage);

                await eventBus.PublishAsync(new AgentStatusEvent
                {
                    EventType = AgentEventType.Error,
                    Status = "Blog generation failed",
                    Error = new ErrorData
                    {
                        ErrorType = "EmptyContent",
                        Message = errorMessage,
                        Retryable = true
                    }
                }, CancellationToken.None).ConfigureAwait(false);

                return;
            }

            blogEntry.Citations = scopedCitationExtractor.ExtractCitations(blogEntry.Content).Citations;
            blogEntry.References = scopedCitationExtractor.ExtractCitations(blogEntry.Content).References;
            blogEntry.QualityAssessment = scopedQualityReviewer.ReviewContent(blogEntry.Content);

            resultStore.SetResult(jobId, blogEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Blog generation job {JobId} failed", jobId);
            resultStore.SetError(jobId, ex.Message);

            await eventBus.PublishAsync(new AgentStatusEvent
            {
                EventType = AgentEventType.Error,
                Status = "Blog generation failed",
                Error = new ErrorData
                {
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    Retryable = false
                }
            }, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            jobAgentService.CompleteJob(jobId);
        }
    }
}
