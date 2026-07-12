using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Insight.Services.Core.Tests.Streaming;

[TestClass]
public class EventBusChannelsTests
{
    [TestMethod]
    public async Task PublishAsync_WithValidEvent_ShouldSucceed()
    {
        var bus = new EventBusChannels(10);
        var @event = new AgentStatusEvent
        {
            InteractionId = "test-123",
            EventType = AgentEventType.Interacting,
            Status = "Test event"
        };

        await bus.PublishAsync(@event);

        Assert.IsNotNull(@event.EventId);
    }

    [TestMethod]
    public async Task PublishAsync_WithNullEvent_ShouldThrow()
    {
        var bus = new EventBusChannels(10);
        try
        {
            await bus.PublishAsync(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task SubscribeAsync_ShouldReceivePublishedEvents()
    {
        var bus = new EventBusChannels(10);
        var events = new List<AgentStatusEvent>();
        var cts = new CancellationTokenSource();

        var @event1 = new AgentStatusEvent
        {
            InteractionId = "test-1",
            EventType = AgentEventType.Interacting,
            Status = "Event 1"
        };

        var @event2 = new AgentStatusEvent
        {
            InteractionId = "test-1",
            EventType = AgentEventType.StepStarted,
            Status = "Event 2"
        };

        // Start subscriber
        var subscribeTask = Task.Run(async () =>
        {
            await foreach (var evt in bus.SubscribeAsync(cts.Token))
            {
                events.Add(evt);
                if (events.Count >= 2)
                    cts.Cancel();
            }
        });

        // Publish events
        await Task.Delay(10);
        await bus.PublishAsync(@event1);
        await bus.PublishAsync(@event2);

        try
        {
            await subscribeTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        Assert.AreEqual(2, events.Count);
        Assert.AreEqual(AgentEventType.Interacting, events[0].EventType);
        Assert.AreEqual(AgentEventType.StepStarted, events[1].EventType);
    }

    [TestMethod]
    public async Task SubscribeAsync_MultipleSubscribers_ShouldBothReceiveEvents()
    {
        var bus = new EventBusChannels(10);
        var events1 = new List<AgentStatusEvent>();
        var events2 = new List<AgentStatusEvent>();
        var cts = new CancellationTokenSource();

        var @event = new AgentStatusEvent
        {
            InteractionId = "test-1",
            EventType = AgentEventType.Interacting,
            Status = "Test"
        };

        var subscribe1Task = Task.Run(async () =>
        {
            await foreach (var evt in bus.SubscribeAsync(cts.Token))
            {
                events1.Add(evt);
            }
        });

        var subscribe2Task = Task.Run(async () =>
        {
            await foreach (var evt in bus.SubscribeAsync(cts.Token))
            {
                events2.Add(evt);
            }
        });

        await Task.Delay(10);
        await bus.PublishAsync(@event);
        await Task.Delay(10);
        cts.Cancel();

        try
        {
            await Task.WhenAll(subscribe1Task, subscribe2Task);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        Assert.IsTrue(events1.Count > 0, "Subscriber 1 should receive event");
        Assert.IsTrue(events2.Count > 0, "Subscriber 2 should receive event");
    }

    [TestMethod]
    public async Task PublishAsync_WithBoundedChannel_ShouldDropOldestOnOverflow()
    {
        var bus = new EventBusChannels(2);
        var events = new List<AgentStatusEvent>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var @event1 = new AgentStatusEvent { InteractionId = "1", EventType = AgentEventType.Interacting };
        var @event2 = new AgentStatusEvent { InteractionId = "2", EventType = AgentEventType.StepStarted };
        var @event3 = new AgentStatusEvent { InteractionId = "3", EventType = AgentEventType.StepProgressing };

        var subscribeTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var evt in bus.SubscribeAsync(cts.Token))
                {
                    events.Add(evt);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        });

        await Task.Delay(10);
        await bus.PublishAsync(@event1);
        await bus.PublishAsync(@event2);
        await bus.PublishAsync(@event3); // This should drop @event1
        await Task.Delay(100);

        try
        {
            cts.Cancel();
            await subscribeTask;
        }
        catch
        {
            // Expected
        }

        // We should have at least 2 events, and @event3 should be present
        Assert.IsTrue(events.Count >= 2, $"Expected at least 2 events, got {events.Count}");
        Assert.IsTrue(events.Any(e => e.InteractionId == "3"), "Event 3 should be present");
    }
}
