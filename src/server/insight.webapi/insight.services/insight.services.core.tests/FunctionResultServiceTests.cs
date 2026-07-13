using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests;

[TestClass]
public class FunctionResultServiceTests
{
    private Mock<IJobAgentService> _jobAgentServiceMock = null!;
    private Mock<IEventBus> _eventBusMock = null!;
    private Mock<ILogger<FunctionResultService>> _loggerMock = null!;
    private IFunctionResultService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _jobAgentServiceMock = new Mock<IJobAgentService>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<FunctionResultService>>();

        _jobAgentServiceMock
            .Setup(x => x.GetEventBus(It.IsAny<string>()))
            .Returns(_eventBusMock.Object);

        _service = new FunctionResultService(_jobAgentServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task RegisterFunctionCallAsync_WithValidDetails_RegistersAndPublishesEvent()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search",
            Arguments = new Dictionary<string, object> { { "query", "test" } },
            StepIndex = 0
        };

        await _service.RegisterFunctionCallAsync(jobId, details);

        var pending = _service.GetPendingFunctionCall(jobId);
        Assert.IsNotNull(pending);
        Assert.AreEqual("func-001", pending.FunctionId);
        Assert.AreEqual("search", pending.FunctionName);

        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<AgentStatusEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisterFunctionCallAsync_WithNullDetails_ThrowsException()
    {
        var jobId = "job-123";

        try
        {
            await _service.RegisterFunctionCallAsync(jobId, null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void GetPendingFunctionCall_WithNoPendingCall_ReturnsNull()
    {
        var pending = _service.GetPendingFunctionCall("nonexistent-job");
        Assert.IsNull(pending);
    }

    [TestMethod]
    public async Task GetPendingFunctionCall_AfterExecution_ReturnsNull()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search",
            Arguments = new Dictionary<string, object>()
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        Assert.IsNotNull(_service.GetPendingFunctionCall(jobId));

        await _service.SubmitFunctionResultAsync(jobId, "func-001", "result data");
        Assert.IsNull(_service.GetPendingFunctionCall(jobId));
    }

    [TestMethod]
    public async Task SubmitFunctionResultAsync_WithValidResult_MarksExecuted()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        var result = await _service.SubmitFunctionResultAsync(jobId, "func-001", "search results");

        Assert.IsTrue(result);
        // After execution, GetPendingFunctionCall returns null (no longer pending)
        Assert.IsNull(_service.GetPendingFunctionCall(jobId));
    }

    [TestMethod]
    public async Task SubmitFunctionResultAsync_WithWrongFunctionId_ReturnsFalse()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        var result = await _service.SubmitFunctionResultAsync(jobId, "func-999", "result data");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task SubmitFunctionResultAsync_PublishesCompletionEvent()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        _eventBusMock.Reset();

        await _service.SubmitFunctionResultAsync(jobId, "func-001", "result data");

        _eventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<AgentStatusEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HasPendingFunctionCall_WithPendingCall_ReturnsTrue()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        Assert.IsTrue(_service.HasPendingFunctionCall(jobId));
    }

    [TestMethod]
    public async Task HasPendingFunctionCall_AfterExecution_ReturnsFalse()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        await _service.SubmitFunctionResultAsync(jobId, "func-001", "result data");

        Assert.IsFalse(_service.HasPendingFunctionCall(jobId));
    }

    [TestMethod]
    public async Task HasPendingFunctionCall_WithNoState_ReturnsFalse()
    {
        Assert.IsFalse(_service.HasPendingFunctionCall("nonexistent-job"));
    }

    [TestMethod]
    public async Task ClearFunctionState_RemovesState()
    {
        var jobId = "job-123";
        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        Assert.IsNotNull(_service.GetPendingFunctionCall(jobId));

        _service.ClearFunctionState(jobId);
        Assert.IsNull(_service.GetPendingFunctionCall(jobId));
    }

    [TestMethod]
    public async Task MultipleFunctionCalls_MaintainsSeparateState()
    {
        var job1 = "job-1";
        var job2 = "job-2";

        var details1 = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search"
        };

        var details2 = new FunctionCallDetails
        {
            FunctionId = "func-002",
            FunctionName = "calculator"
        };

        await _service.RegisterFunctionCallAsync(job1, details1);
        await _service.RegisterFunctionCallAsync(job2, details2);

        var pending1 = _service.GetPendingFunctionCall(job1);
        var pending2 = _service.GetPendingFunctionCall(job2);

        Assert.IsNotNull(pending1);
        Assert.IsNotNull(pending2);
        Assert.AreEqual("func-001", pending1.FunctionId);
        Assert.AreEqual("func-002", pending2.FunctionId);
    }

    [TestMethod]
    public async Task FunctionCallWithArguments_PreservesArguments()
    {
        var jobId = "job-123";
        var args = new Dictionary<string, object>
        {
            { "query", "test search" },
            { "limit", 10 }
        };

        var details = new FunctionCallDetails
        {
            FunctionId = "func-001",
            FunctionName = "search",
            Arguments = args
        };

        await _service.RegisterFunctionCallAsync(jobId, details);
        var pending = _service.GetPendingFunctionCall(jobId);

        Assert.IsNotNull(pending?.Arguments);
        Assert.AreEqual(2, pending.Arguments.Count);
        Assert.AreEqual("test search", pending.Arguments["query"]);
    }

    [TestMethod]
    public async Task SubmitFunctionResultAsync_WithNullJobId_ThrowsException()
    {
        try
        {
            await _service.SubmitFunctionResultAsync(null!, "func-001", "result");
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }
}
