namespace Insight.Services.Interfaces.Ai;

public interface ICanReadAgentDefinition<TAgentDefinition> where TAgentDefinition : class
{
    IDictionary<string, TAgentDefinition> Load(string agentFolder);
    TAgentDefinition GetAgent(string agentName);
}


