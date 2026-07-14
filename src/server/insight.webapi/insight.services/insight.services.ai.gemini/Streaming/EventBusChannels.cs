using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Insight.Services.Interfaces.Ai.Events;

namespace Insight.Services.Ai.Gemini.Streaming;

public class EventBusChannels : IEventBus
{
    private readonly Channel<AgentStatusEvent> _channel;

    public EventBusChannels(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<AgentStatusEvent>(options);
    }

    public async ValueTask PublishAsync(AgentStatusEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }

    public async IAsyncEnumerable<AgentStatusEvent> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var @event in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return @event;
        }
    }

    public void Complete()
    {
        _channel.Writer.TryComplete();
    }

    public int? QueueDepth => _channel.Reader.CanCount ? _channel.Reader.Count : null;
}
