using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Insight.Services.Core.Tests;

[TestClass]
public class BlogServiceJobTests
{
    private Mock<IBlogAgent> _mockBlogAgent;
    private Mock<ICitationExtractor> _mockCitationExtractor;
    private Mock<IContentQualityReviewer> _mockQualityReviewer;
    private Mock<IJobAgentService> _mockJobAgentService;
    private Mock<IEventBus> _mockEventBus;
    private BlogJobResultStore _resultStore;
    private ServiceProvider _serviceProvider;
    private BlogService _blogService;

    [TestInitialize]
    public void Setup()
    {
        _mockBlogAgent = new Mock<IBlogAgent>();
        _mockCitationExtractor = new Mock<ICitationExtractor>();
        _mockQualityReviewer = new Mock<IContentQualityReviewer>();
        _mockJobAgentService = new Mock<IJobAgentService>();
        _mockEventBus = new Mock<IEventBus>();
        _resultStore = new BlogJobResultStore();

        _mockJobAgentService
            .Setup(x => x.GetOrCreateEventBus(It.IsAny<string>()))
            .Returns(_mockEventBus.Object);

        // Real DI container so the background job's scoped resolution returns these same mocks.
        var services = new ServiceCollection();
        services.AddSingleton(_mockBlogAgent.Object);
        services.AddSingleton(_mockCitationExtractor.Object);
        services.AddSingleton(_mockQualityReviewer.Object);
        _serviceProvider = services.BuildServiceProvider();

        _blogService = new BlogService(
            _mockBlogAgent.Object,
            _mockCitationExtractor.Object,
            _mockQualityReviewer.Object,
            _mockJobAgentService.Object,
            _resultStore,
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Mock.Of<ILogger<BlogService>>()
        );
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_ReturnsNonEmptyJobId()
    {
        _mockBlogAgent
            .Setup(x => x.CreateBlogPostStreamedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEventBus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogEntry { Content = "content" });

        var jobId = await _blogService.StartBlogEntryJobAsync("Test Topic", "audience", "style");

        Assert.IsFalse(string.IsNullOrWhiteSpace(jobId));
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_RegistersEventBusForJobId()
    {
        _mockBlogAgent
            .Setup(x => x.CreateBlogPostStreamedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEventBus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogEntry { Content = "content" });

        var jobId = await _blogService.StartBlogEntryJobAsync("Test Topic", "audience", "style");

        _mockJobAgentService.Verify(x => x.GetOrCreateEventBus(jobId), Times.Once);
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_WithEmptyTopic_ThrowsArgumentException()
    {
        try
        {
            await _blogService.StartBlogEntryJobAsync("", "audience", "style");
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_OnSuccess_StoresResultAndCompletesJob()
    {
        var completionSignal = new TaskCompletionSource();

        _mockCitationExtractor.Setup(x => x.ExtractCitations(It.IsAny<string>())).Returns(new CitationInfo());
        _mockQualityReviewer.Setup(x => x.ReviewContent(It.IsAny<string>())).Returns(new ContentQualityAssessment { QualityScore = 1m });
        _mockBlogAgent
            .Setup(x => x.CreateBlogPostStreamedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEventBus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogEntry { Content = "Generated content" });
        _mockJobAgentService
            .Setup(x => x.CompleteJob(It.IsAny<string>()))
            .Callback(() => completionSignal.TrySetResult());

        var jobId = await _blogService.StartBlogEntryJobAsync("Test Topic", "audience", "style");

        var finished = await Task.WhenAny(completionSignal.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.AreSame(completionSignal.Task, finished, "Background job did not complete in time");

        var result = _blogService.GetJobResult(jobId);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Generated content", result.Entry!.Content);
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_OnFailure_StoresErrorAndPublishesErrorEvent()
    {
        var completionSignal = new TaskCompletionSource();

        _mockBlogAgent
            .Setup(x => x.CreateBlogPostStreamedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEventBus>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("generation failed"));
        _mockJobAgentService
            .Setup(x => x.CompleteJob(It.IsAny<string>()))
            .Callback(() => completionSignal.TrySetResult());

        var jobId = await _blogService.StartBlogEntryJobAsync("Test Topic", "audience", "style");

        var finished = await Task.WhenAny(completionSignal.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.AreSame(completionSignal.Task, finished, "Background job did not complete in time");

        var result = _blogService.GetJobResult(jobId);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("generation failed", result.Error);

        _mockEventBus.Verify(
            x => x.PublishAsync(It.Is<AgentStatusEvent>(e => e.EventType == AgentEventType.Error), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public void GetJobResult_UnknownJob_ReturnsNull()
    {
        Assert.IsNull(_blogService.GetJobResult("nonexistent-job"));
    }

    [TestMethod]
    public async Task StartBlogEntryJobAsync_WhenGeneratedContentIsEmpty_StoresErrorAndPublishesErrorEvent()
    {
        var completionSignal = new TaskCompletionSource();

        _mockBlogAgent
            .Setup(x => x.CreateBlogPostStreamedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEventBus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogEntry { Content = string.Empty });
        _mockJobAgentService
            .Setup(x => x.CompleteJob(It.IsAny<string>()))
            .Callback(() => completionSignal.TrySetResult());

        var jobId = await _blogService.StartBlogEntryJobAsync("Test Topic", "audience", "style");

        var finished = await Task.WhenAny(completionSignal.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.AreSame(completionSignal.Task, finished, "Background job did not complete in time");

        var result = _blogService.GetJobResult(jobId);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess, "A job with empty generated content should not be reported as successful");
        Assert.IsNotNull(result.Error);

        _mockEventBus.Verify(
            x => x.PublishAsync(It.Is<AgentStatusEvent>(e => e.EventType == AgentEventType.Error), It.IsAny<CancellationToken>()),
            Times.Once);

        // Quality review should never run against empty content that's already being treated as a failure.
        _mockQualityReviewer.Verify(x => x.ReviewContent(It.IsAny<string>()), Times.Never);
    }
}
