using Insight.Services.Interfaces.Ai;

namespace Insight.WebApi.Models;

public class JobProgressDto
{
    public string JobId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;

    // Overall metrics
    public int TotalWordCount { get; set; }
    public int TotalCharacterCount { get; set; }
    public int TotalParagraphCount { get; set; }

    // Step tracking
    public int CurrentStep { get; set; }
    public int? TotalSteps { get; set; }
    public double CompletionPercentage { get; set; }

    // Time metrics
    public double ElapsedSeconds { get; set; }
    public string ElapsedTimeFormatted { get; set; } = string.Empty;

    // Token metrics
    public long? TotalInputTokens { get; set; }
    public long? TotalOutputTokens { get; set; }
    public long? TotalCachedTokens { get; set; }
    public long? TotalThoughtTokens { get; set; }
    public long? TotalTokens { get; set; }

    // Status
    public bool IsComplete { get; set; }

    // Calculations
    public double WordsPerSecond { get; set; }
    public double TokensPerSecond { get; set; }

    public static JobProgressDto FromDomain(JobProgressMetrics metrics)
    {
        var elapsed = TimeSpan.FromSeconds(metrics.ElapsedSeconds);
        var wordsPerSecond = metrics.ElapsedSeconds > 0 ? metrics.TotalWordCount / metrics.ElapsedSeconds : 0;
        var totalTokens = (metrics.TotalInputTokens ?? 0) + (metrics.TotalOutputTokens ?? 0);
        var tokensPerSecond = metrics.ElapsedSeconds > 0 ? totalTokens / metrics.ElapsedSeconds : 0;

        return new JobProgressDto
        {
            JobId = metrics.JobId,
            StartTime = metrics.StartTime,
            LastUpdated = metrics.LastUpdated,
            CurrentStatus = metrics.CurrentStatus,
            TotalWordCount = metrics.TotalWordCount,
            TotalCharacterCount = metrics.TotalCharacterCount,
            TotalParagraphCount = metrics.TotalParagraphCount,
            CurrentStep = metrics.CurrentStep,
            TotalSteps = metrics.TotalSteps,
            CompletionPercentage = metrics.CompletionPercentage,
            ElapsedSeconds = metrics.ElapsedSeconds,
            ElapsedTimeFormatted = FormatTimeSpan(elapsed),
            TotalInputTokens = metrics.TotalInputTokens,
            TotalOutputTokens = metrics.TotalOutputTokens,
            TotalCachedTokens = metrics.TotalCachedTokens,
            TotalThoughtTokens = metrics.TotalThoughtTokens,
            TotalTokens = metrics.TotalTokens,
            IsComplete = metrics.IsComplete,
            WordsPerSecond = Math.Round(wordsPerSecond, 2),
            TokensPerSecond = Math.Round(tokensPerSecond, 2)
        };
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalSeconds < 60)
            return $"{ts.TotalSeconds:F1}s";
        if (ts.TotalMinutes < 60)
            return $"{ts.TotalMinutes:F1}m";
        return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
    }
}

public class DetailedJobProgressDto
{
    public JobProgressDto CurrentMetrics { get; set; } = null!;
    public List<StepMetricsDto> StepHistory { get; set; } = new();
    public List<string> EventLog { get; set; } = new();

    public static DetailedJobProgressDto FromDomain(DetailedJobProgress progress)
    {
        return new DetailedJobProgressDto
        {
            CurrentMetrics = JobProgressDto.FromDomain(progress.CurrentMetrics),
            StepHistory = progress.StepHistory
                .Select(s => StepMetricsDto.FromDomain(s))
                .ToList(),
            EventLog = progress.Events ?? new()
        };
    }
}

public class StepMetricsDto
{
    public int StepIndex { get; set; }
    public string StepType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double ElapsedSeconds { get; set; }

    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public int ParagraphCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public static StepMetricsDto FromDomain(StepMetrics metrics)
    {
        return new StepMetricsDto
        {
            StepIndex = metrics.StepIndex,
            StepType = metrics.StepType,
            StartTime = metrics.StartTime,
            EndTime = metrics.EndTime,
            ElapsedSeconds = metrics.ElapsedSeconds,
            WordCount = metrics.WordCount,
            CharacterCount = metrics.CharacterCount,
            ParagraphCount = metrics.ParagraphCount,
            Status = metrics.Status
        };
    }
}
