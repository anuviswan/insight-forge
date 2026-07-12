namespace Insight.Services.Interfaces.Ai.Events;

public class AgentStatusEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string InteractionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AgentEventType EventType { get; set; }
    public string Status { get; set; } = string.Empty;
    public ProgressData? Progress { get; set; }
    public ErrorData? Error { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
