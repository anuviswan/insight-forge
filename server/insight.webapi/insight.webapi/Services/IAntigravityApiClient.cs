using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public interface IAntigravityApiClient
    {
        Task<string?> RunAgentWorkflowAsync(string agentName, string workflow, string input, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default);
    }
}
