using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests;

[TestClass]
public class ProgressMetricsServiceTests
{
    private Mock<ILogger<ProgressMetricsService>> _loggerMock = null!;
    private IProgressMetricsService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ProgressMetricsService>>();
        _service = new ProgressMetricsService(_loggerMock.Object);
    }

    [TestMethod]
    public void TrackEvent_WithValidEvent_ShouldUpdateMetrics()
    {
        var jobId = "job-123";
        var @event = new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Status = "Starting",
            Progress = new ProgressData
            {
                WordCount = 0,
                CharacterCount = 0,
                ParagraphCount = 0,
                ElapsedTime = TimeSpan.Zero
            }
        };

        _service.TrackEvent(jobId, @event);
        var metrics = _service.GetProgress(jobId);

        Assert.IsNotNull(metrics);
        Assert.AreEqual(jobId, metrics.JobId);
        Assert.AreEqual(0, metrics.TotalWordCount);
    }

    [TestMethod]
    public void TrackEvent_MultipleEvents_ShouldAccumulateMetrics()
    {
        var jobId = "job-123";

        // Event 1: Start
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Status = "Starting"
        });

        // Event 2: Step started
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepStarted,
            Status = "Step 0 started",
            Data = new Dictionary<string, object> { { "step_type", "model_output" } }
        });

        // Event 3: Step progressing (accumulating content)
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepProgressing,
            Status = "Generating",
            Progress = new ProgressData
            {
                CurrentStep = 0,
                WordCount = 5,
                CharacterCount = 28,
                ParagraphCount = 1,
                ElapsedTime = TimeSpan.FromSeconds(0.5)
            }
        });

        // Event 4: More progress
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepProgressing,
            Status = "Generating",
            Progress = new ProgressData
            {
                CurrentStep = 0,
                WordCount = 10,
                CharacterCount = 56,
                ParagraphCount = 2,
                ElapsedTime = TimeSpan.FromSeconds(1.0)
            }
        });

        var metrics = _service.GetProgress(jobId);

        Assert.AreEqual(10, metrics.TotalWordCount);
        Assert.AreEqual(56, metrics.TotalCharacterCount);
        Assert.AreEqual(2, metrics.TotalParagraphCount);
        Assert.IsTrue(metrics.ElapsedSeconds > 0);
    }

    [TestMethod]
    public void GetProgress_WithNoEvents_ShouldReturnNull()
    {
        var metrics = _service.GetProgress("nonexistent-job");

        Assert.IsNull(metrics);
    }

    [TestMethod]
    public void GetProgress_WithTrackedJob_ShouldReturnMetrics()
    {
        var jobId = "job-123";
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Progress = new ProgressData
            {
                WordCount = 42,
                CharacterCount = 237,
                ParagraphCount = 3
            }
        });

        var metrics = _service.GetProgress(jobId);

        Assert.IsNotNull(metrics);
        Assert.AreEqual(42, metrics.TotalWordCount);
        Assert.AreEqual(237, metrics.TotalCharacterCount);
        Assert.AreEqual(3, metrics.TotalParagraphCount);
    }

    [TestMethod]
    public void TrackEvent_WithTokens_ShouldTrackTokenCounts()
    {
        var jobId = "job-123";

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.InteractionComplete,
            Progress = new ProgressData
            {
                WordCount = 100,
                CharacterCount = 500,
                ParagraphCount = 5,
                ElapsedTime = TimeSpan.FromSeconds(2),
                TotalInputTokens = 120,
                TotalOutputTokens = 80,
                TotalCachedTokens = 50,
                TotalThoughtTokens = 10
            }
        });

        var metrics = _service.GetProgress(jobId);

        Assert.AreEqual(120, metrics.TotalInputTokens);
        Assert.AreEqual(80, metrics.TotalOutputTokens);
        Assert.AreEqual(50, metrics.TotalCachedTokens);
        Assert.AreEqual(10, metrics.TotalThoughtTokens);
        Assert.AreEqual(200, metrics.TotalTokens); // 120 + 80
    }

    [TestMethod]
    public void GetProgress_WithCompleteEvent_ShouldMarkComplete()
    {
        var jobId = "job-123";

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.InteractionComplete,
            Status = "Complete"
        });

        var metrics = _service.GetProgress(jobId);

        Assert.IsTrue(metrics.IsComplete);
    }

    [TestMethod]
    public void GetDetailedProgress_ShouldIncludeStepHistory()
    {
        var jobId = "job-123";

        // Step 0
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepStarted,
            Data = new Dictionary<string, object> { { "step_type", "model_output" } }
        });

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepProgressing,
            Progress = new ProgressData
            {
                CurrentStep = 0,
                WordCount = 25,
                CharacterCount = 150,
                ParagraphCount = 1
            }
        });

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepCompleted,
            Progress = new ProgressData
            {
                CurrentStep = 0,
                WordCount = 50,
                CharacterCount = 300,
                ParagraphCount = 2
            }
        });

        // Step 1
        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepStarted,
            Data = new Dictionary<string, object> { { "step_type", "model_output" } }
        });

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepCompleted,
            Progress = new ProgressData
            {
                CurrentStep = 1,
                WordCount = 75,
                CharacterCount = 450,
                ParagraphCount = 3
            }
        });

        var detailed = _service.GetDetailedProgress(jobId);

        Assert.IsNotNull(detailed);
        Assert.IsTrue(detailed.StepHistory.Count >= 2);
        Assert.IsTrue(detailed.Events.Count >= 5);
    }

    [TestMethod]
    public void GetProgress_WithStepInfo_ShouldCalculateProgress()
    {
        var jobId = "job-123";

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.StepProgressing,
            Progress = new ProgressData
            {
                CurrentStep = 2,
                TotalSteps = 4
            }
        });

        var metrics = _service.GetProgress(jobId);

        Assert.AreEqual(2, metrics.CurrentStep);
        Assert.AreEqual(4, metrics.TotalSteps);
        Assert.AreEqual(50, metrics.CompletionPercentage); // 2/4 = 50%
    }

    [TestMethod]
    public void ClearMetrics_ShouldRemoveJobData()
    {
        var jobId = "job-123";

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Progress = new ProgressData { WordCount = 100 }
        });

        var metricsBefore = _service.GetProgress(jobId);
        Assert.IsNotNull(metricsBefore);

        _service.ClearMetrics(jobId);

        var metricsAfter = _service.GetProgress(jobId);
        Assert.IsNull(metricsAfter);
    }

    [TestMethod]
    public void TrackEvent_WithError_ShouldRecordInLog()
    {
        var jobId = "job-123";

        _service.TrackEvent(jobId, new AgentStatusEvent
        {
            EventType = AgentEventType.Error,
            Status = "Error occurred",
            Error = new ErrorData { ErrorType = "RuntimeError", Message = "Test error" }
        });

        var detailed = _service.GetDetailedProgress(jobId);

        Assert.IsTrue(detailed.Events.Any(e => e.Contains("Error")));
    }

    [TestMethod]
    public void GetProgress_WithMultipleJobs_ShouldReturnCorrectMetrics()
    {
        var jobId1 = "job-1";
        var jobId2 = "job-2";

        _service.TrackEvent(jobId1, new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Progress = new ProgressData { WordCount = 100 }
        });

        _service.TrackEvent(jobId2, new AgentStatusEvent
        {
            EventType = AgentEventType.Interacting,
            Progress = new ProgressData { WordCount = 200 }
        });

        var metrics1 = _service.GetProgress(jobId1);
        var metrics2 = _service.GetProgress(jobId2);

        Assert.AreEqual(100, metrics1.TotalWordCount);
        Assert.AreEqual(200, metrics2.TotalWordCount);
    }
}
