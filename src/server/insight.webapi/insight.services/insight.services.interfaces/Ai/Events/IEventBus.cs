namespace Insight.Services.Interfaces.Ai.Events;

public interface IEventBus
{
    ValueTask PublishAsync(AgentStatusEvent @event, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AgentStatusEvent> SubscribeAsync(CancellationToken cancellationToken = default);
}
