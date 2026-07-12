namespace Insight.Services.Interfaces.Ai.Events;

public interface IEventTransformer<in TSource, out TTarget> where TTarget : AgentStatusEvent
{
    TTarget Transform(TSource source);
}
