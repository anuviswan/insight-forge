using System.Threading;
using System.Threading.Tasks;

namespace Insight.Services.Interfaces.Ai;

public interface IAgentMetadataProvider<T> where T : class
{
    Task<T> GetAgentDefinitionAsync(string agentFolder, CancellationToken cancellationToken = default);
}
