using System.Threading;
using System.Threading.Tasks;

namespace Insight.Services.Interfaces.Ai;

public interface IAgentMetadataProvider<TAgentDefinition,TSkillDefinition, TWorkflowDefinition> : ICanReadAgentDefinition<TAgentDefinition>, ICanReadSkillDefinition<TSkillDefinition>, ICanReadWorkflowDefinition<TWorkflowDefinition>
    where TAgentDefinition : class
    where TSkillDefinition : class
    where TWorkflowDefinition : class
{
    Task<TAgentDefinition> GetAgentDefinitionAsync(string agentFolder, CancellationToken cancellationToken = default);
}
