namespace Insight.Services.Interfaces.Ai;

public interface ICanReadAgentDefinition<TAgentDefinition> where TAgentDefinition : class
{
    IDictionary<string, TAgentDefinition> LoadAgents();
    TAgentDefinition GetAgent(string agentName);
}


