using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests;

[TestClass]
public class JobAgentServiceTests
{
    private Mock<ILogger<JobAgentService>> _loggerMock = null!;
    private IJobAgentService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<JobAgentService>>();
        _service = new JobAgentService(_loggerMock.Object);
    }

    [TestMethod]
    public void GetOrCreateEventBus_WithNewJobId_ShouldCreateEventBus()
    {
        var jobId = "job-123";

        var eventBus = _service.GetOrCreateEventBus(jobId);

        Assert.IsNotNull(eventBus);
        Assert.IsTrue(_service.IsJobActive(jobId));
    }

    [TestMethod]
    public void GetOrCreateEventBus_WithSameJobId_ShouldReturnSameEventBus()
    {
        var jobId = "job-123";

        var eventBus1 = _service.GetOrCreateEventBus(jobId);
        var eventBus2 = _service.GetOrCreateEventBus(jobId);

        Assert.AreSame(eventBus1, eventBus2);
    }

    [TestMethod]
    public void GetOrCreateEventBus_WithNullJobId_ShouldThrow()
    {
        try
        {
            _service.GetOrCreateEventBus(null!);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void GetEventBus_WithActiveJob_ShouldReturnEventBus()
    {
        var jobId = "job-123";
        _service.GetOrCreateEventBus(jobId);

        var eventBus = _service.GetEventBus(jobId);

        Assert.IsNotNull(eventBus);
    }

    [TestMethod]
    public void GetEventBus_WithInactiveJob_ShouldReturnNull()
    {
        var eventBus = _service.GetEventBus("nonexistent-job");

        Assert.IsNull(eventBus);
    }

    [TestMethod]
    public void CompleteJob_WithActiveJob_ShouldRemoveJob()
    {
        var jobId = "job-123";
        _service.GetOrCreateEventBus(jobId);

        _service.CompleteJob(jobId);

        Assert.IsFalse(_service.IsJobActive(jobId));
        Assert.IsNull(_service.GetEventBus(jobId));
    }

    [TestMethod]
    public void IsJobActive_WithActiveJob_ShouldReturnTrue()
    {
        var jobId = "job-123";
        _service.GetOrCreateEventBus(jobId);

        var isActive = _service.IsJobActive(jobId);

        Assert.IsTrue(isActive);
    }

    [TestMethod]
    public void IsJobActive_WithInactiveJob_ShouldReturnFalse()
    {
        var isActive = _service.IsJobActive("nonexistent-job");

        Assert.IsFalse(isActive);
    }

    [TestMethod]
    public void GetActiveJobs_WithMultipleJobs_ShouldReturnAllJobIds()
    {
        var jobId1 = "job-1";
        var jobId2 = "job-2";
        var jobId3 = "job-3";

        _service.GetOrCreateEventBus(jobId1);
        _service.GetOrCreateEventBus(jobId2);
        _service.GetOrCreateEventBus(jobId3);

        var activeJobs = _service.GetActiveJobs().ToList();

        Assert.AreEqual(3, activeJobs.Count);
        Assert.IsTrue(activeJobs.Contains(jobId1));
        Assert.IsTrue(activeJobs.Contains(jobId2));
        Assert.IsTrue(activeJobs.Contains(jobId3));
    }

    [TestMethod]
    public void GetActiveJobs_WithNoJobs_ShouldReturnEmpty()
    {
        var activeJobs = _service.GetActiveJobs();

        Assert.AreEqual(0, activeJobs.Count());
    }

    [TestMethod]
    public void CompleteJob_WithMultipleJobs_ShouldOnlyRemoveCompletedJob()
    {
        var jobId1 = "job-1";
        var jobId2 = "job-2";

        _service.GetOrCreateEventBus(jobId1);
        _service.GetOrCreateEventBus(jobId2);

        _service.CompleteJob(jobId1);

        var activeJobs = _service.GetActiveJobs().ToList();
        Assert.AreEqual(1, activeJobs.Count);
        Assert.IsTrue(activeJobs.Contains(jobId2));
        Assert.IsFalse(_service.IsJobActive(jobId1));
    }

    [TestMethod]
    public async Task EventBus_PublishAndSubscribe_ShouldWorkCorrectly()
    {
        var jobId = "job-123";
        var eventBus = _service.GetOrCreateEventBus(jobId);
        var receivedEvents = new List<AgentStatusEvent>();

        var subscribeTask = Task.Run(async () =>
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            try
            {
                await foreach (var @event in eventBus.SubscribeAsync(cts.Token))
                {
                    receivedEvents.Add(@event);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        });

        // Give subscriber time to start
        await Task.Delay(50);

        // Publish events
        var event1 = new AgentStatusEvent { EventType = AgentEventType.Interacting };
        var event2 = new AgentStatusEvent { EventType = AgentEventType.StepStarted };

        await eventBus.PublishAsync(event1);
        await eventBus.PublishAsync(event2);

        // Wait for subscriber
        await subscribeTask;

        Assert.IsTrue(receivedEvents.Count >= 2);
    }
}
