using Insight.Services.Ai.Gemini.Resilience;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests.Resilience;

[TestClass]
public class StreamingResilienceMetricsServiceTests
{
    private Mock<IJobAgentService> _jobAgentServiceMock = null!;
    private StreamingResilienceMetricsService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _jobAgentServiceMock = new Mock<IJobAgentService>();
        _jobAgentServiceMock.Setup(x => x.GetActiveJobs()).Returns(Enumerable.Empty<string>());
        _service = new StreamingResilienceMetricsService(_jobAgentServiceMock.Object);
    }

    [TestMethod]
    public void GetSnapshot_WithNoActivity_ReturnsEmptyMetrics()
    {
        var snapshot = _service.GetSnapshot();

        Assert.AreEqual(0, snapshot.ErrorsByType.Count);
        Assert.AreEqual(0, snapshot.TotalRetryAttempts);
        Assert.AreEqual(0, snapshot.TotalRetrySuccesses);
        Assert.AreEqual(0, snapshot.RetrySuccessRate);
    }

    [TestMethod]
    public void RecordError_AccumulatesByOperationAndType()
    {
        _service.RecordError("RunAgentInteraction", "ServiceUnavailable");
        _service.RecordError("RunAgentInteraction", "ServiceUnavailable");
        _service.RecordError("StreamAgentInteraction", "Timeout");

        var snapshot = _service.GetSnapshot();

        Assert.AreEqual(2, snapshot.ErrorsByType["RunAgentInteraction:ServiceUnavailable"]);
        Assert.AreEqual(1, snapshot.ErrorsByType["StreamAgentInteraction:Timeout"]);
    }

    [TestMethod]
    public void RecordRetryAttemptAndSuccess_ComputesRetrySuccessRate()
    {
        _service.RecordRetryAttempt("op");
        _service.RecordRetryAttempt("op");
        _service.RecordRetryAttempt("op");
        _service.RecordRetryAttempt("op");
        _service.RecordRetrySuccess("op");

        var snapshot = _service.GetSnapshot();

        Assert.AreEqual(4, snapshot.TotalRetryAttempts);
        Assert.AreEqual(1, snapshot.TotalRetrySuccesses);
        Assert.AreEqual(0.25, snapshot.RetrySuccessRate);
    }

    [TestMethod]
    public void GetSnapshot_IncludesQueueDepthForActiveJobs()
    {
        var eventBusMock = new Mock<IEventBus>();
        eventBusMock.Setup(x => x.QueueDepth).Returns(7);

        _jobAgentServiceMock.Setup(x => x.GetActiveJobs()).Returns(new[] { "job-1" });
        _jobAgentServiceMock.Setup(x => x.GetEventBus("job-1")).Returns(eventBusMock.Object);

        var snapshot = _service.GetSnapshot();

        Assert.AreEqual(7, snapshot.ActiveJobQueueDepths["job-1"]);
    }

    [TestMethod]
    public void GetSnapshot_JobWithNoEventBus_ReportsNullQueueDepth()
    {
        _jobAgentServiceMock.Setup(x => x.GetActiveJobs()).Returns(new[] { "job-gone" });
        _jobAgentServiceMock.Setup(x => x.GetEventBus("job-gone")).Returns((IEventBus?)null);

        var snapshot = _service.GetSnapshot();

        Assert.IsNull(snapshot.ActiveJobQueueDepths["job-gone"]);
    }
}
