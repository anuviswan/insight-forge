using Insight.Services.Interfaces.Ai.Events;

namespace Insight.WebApi.Models;

public class AgentStatusEventDto
{
    public Guid EventId { get; set; }
    public string InteractionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ProgressDataDto? Progress { get; set; }
    public ErrorDataDto? Error { get; set; }
    public Dictionary<string, object>? Data { get; set; }

    public static AgentStatusEventDto FromDomain(AgentStatusEvent domainEvent)
    {
        return new AgentStatusEventDto
        {
            EventId = domainEvent.EventId,
            InteractionId = domainEvent.InteractionId,
            Timestamp = domainEvent.Timestamp,
            EventType = domainEvent.EventType.ToString(),
            Status = domainEvent.Status,
            Progress = domainEvent.Progress != null ? ProgressDataDto.FromDomain(domainEvent.Progress) : null,
            Error = domainEvent.Error != null ? ErrorDataDto.FromDomain(domainEvent.Error) : null,
            Data = domainEvent.Data
        };
    }
}

public class ProgressDataDto
{
    public int CurrentStep { get; set; }
    public int? TotalSteps { get; set; }
    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public int ParagraphCount { get; set; }
    public double ElapsedTimeSeconds { get; set; }
    public long? TotalInputTokens { get; set; }
    public long? TotalOutputTokens { get; set; }
    public long? TotalCachedTokens { get; set; }
    public long? TotalThoughtTokens { get; set; }

    public static ProgressDataDto FromDomain(Insight.Services.Interfaces.Ai.Events.ProgressData domainData)
    {
        return new ProgressDataDto
        {
            CurrentStep = domainData.CurrentStep,
            TotalSteps = domainData.TotalSteps,
            WordCount = domainData.WordCount,
            CharacterCount = domainData.CharacterCount,
            ParagraphCount = domainData.ParagraphCount,
            ElapsedTimeSeconds = domainData.ElapsedTime.TotalSeconds,
            TotalInputTokens = domainData.TotalInputTokens,
            TotalOutputTokens = domainData.TotalOutputTokens,
            TotalCachedTokens = domainData.TotalCachedTokens,
            TotalThoughtTokens = domainData.TotalThoughtTokens
        };
    }
}

public class ErrorDataDto
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Retryable { get; set; }

    public static ErrorDataDto FromDomain(Insight.Services.Interfaces.Ai.Events.ErrorData domainData)
    {
        return new ErrorDataDto
        {
            ErrorType = domainData.ErrorType,
            Message = domainData.Message,
            Retryable = domainData.Retryable
        };
    }
}
