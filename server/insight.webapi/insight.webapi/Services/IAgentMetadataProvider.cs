using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public interface IAgentMetadataProvider
    {
        Task<AgentDefinitionDto> GetAgentDefinitionAsync(string agentFolder, CancellationToken cancellationToken = default);
    }
}
