using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Insight.Services.Interfaces.Ai.Events;

namespace Insight.Services.Ai.Gemini.Streaming;

/// <summary>
/// In-memory pub/sub event bus backed by bounded channels. Each call to
/// <see cref="SubscribeAsync"/> gets its own channel so every subscriber
/// independently receives all published events (fan-out), rather than
/// subscribers competing for events from a single shared channel.
/// </summary>
public class EventBusChannels : IEventBus
{
    private readonly int _capacity;
    private readonly ConcurrentDictionary<Channel<AgentStatusEvent>, byte> _subscribers = new();
    private volatile bool _completed;

    public EventBusChannels(int capacity = 100)
    {
        _capacity = capacity;
    }

    public async ValueTask PublishAsync(AgentStatusEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        foreach (var subscriber in _subscribers.Keys)
        {
            await subscriber.Writer.WriteAsync(@event, cancellationToken);
        }
    }

    public async IAsyncEnumerable<AgentStatusEvent> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var options = new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        var channel = Channel.CreateBounded<AgentStatusEvent>(options);

        _subscribers[channel] = 0;
        if (_completed)
        {
            channel.Writer.TryComplete();
        }

        try
        {
            await foreach (var @event in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return @event;
            }
        }
        finally
        {
            _subscribers.TryRemove(channel, out _);
        }
    }

    public void Complete()
    {
        _completed = true;
        foreach (var subscriber in _subscribers.Keys)
        {
            subscriber.Writer.TryComplete();
        }
    }

    public int? QueueDepth
    {
        get
        {
            var counted = _subscribers.Keys.Where(c => c.Reader.CanCount).ToList();
            return counted.Count > 0 ? counted.Max(c => c.Reader.Count) : null;
        }
    }
}
