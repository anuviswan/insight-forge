using Insight.Services.Interfaces.Ai.Events;

namespace Insight.Services.Interfaces.Ai;

public interface IProgressMetricsService
{
    /// <summary>
    /// Track an agent status event and update metrics
    /// </summary>
    void TrackEvent(string jobId, AgentStatusEvent @event);

    /// <summary>
    /// Get current progress metrics for a job
    /// </summary>
    JobProgressMetrics? GetProgress(string jobId);

    /// <summary>
    /// Get all metrics for a job including step history
    /// </summary>
    DetailedJobProgress? GetDetailedProgress(string jobId);

    /// <summary>
    /// Clear metrics for a completed job
    /// </summary>
    void ClearMetrics(string jobId);
}

public class JobProgressMetrics
{
    public string JobId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;

    // Overall content metrics
    public int TotalWordCount { get; set; }
    public int TotalCharacterCount { get; set; }
    public int TotalParagraphCount { get; set; }

    // Step tracking
    public int CurrentStep { get; set; }
    public int? TotalSteps { get; set; }

    // Time tracking
    public double ElapsedSeconds { get; set; }
    public double? EstimatedTotalSeconds { get; set; }
    public double? EstimatedRemainingSeconds { get; set; }

    // Token tracking
    public long? TotalInputTokens { get; set; }
    public long? TotalOutputTokens { get; set; }
    public long? TotalCachedTokens { get; set; }
    public long? TotalThoughtTokens { get; set; }
    public long? TotalTokens => (TotalInputTokens ?? 0) + (TotalOutputTokens ?? 0);

    // Completion status
    public bool IsComplete { get; set; }
    public double CompletionPercentage
    {
        get
        {
            if (TotalSteps.HasValue && TotalSteps > 0)
                return (CurrentStep / (double)TotalSteps.Value) * 100;
            return 0;
        }
    }
}

public class DetailedJobProgress
{
    public JobProgressMetrics CurrentMetrics { get; set; } = null!;
    public List<StepMetrics> StepHistory { get; set; } = new();
    public List<string> Events { get; set; } = new();
}

public class StepMetrics
{
    public int StepIndex { get; set; }
    public string StepType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double ElapsedSeconds
    {
        get
        {
            var end = EndTime ?? DateTime.UtcNow;
            return (end - StartTime).TotalSeconds;
        }
    }

    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public int ParagraphCount { get; set; }

    public string Status { get; set; } = string.Empty;
}
