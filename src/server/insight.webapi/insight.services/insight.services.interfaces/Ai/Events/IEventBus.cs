namespace Insight.Services.Interfaces.Ai.Events;

public interface IEventBus
{
    ValueTask PublishAsync(AgentStatusEvent @event, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AgentStatusEvent> SubscribeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Number of events currently buffered and not yet consumed, or null if the
    /// underlying implementation cannot report this cheaply.
    /// </summary>
    int? QueueDepth { get; }
}
