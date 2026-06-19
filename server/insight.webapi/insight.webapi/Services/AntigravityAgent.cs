using System;
using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public class AntigravityAgent : IAntigravityAgent
    {
        private readonly IAntigravityApiClient _apiClient;
        private readonly IAgentMetadataProvider _metadataProvider;
        private const string AgentName = "Blog Writer Agent";
        private const string Workflow = "create-blogpost";

        public AntigravityAgent(IAntigravityApiClient apiClient, IAgentMetadataProvider metadataProvider)
        {
            _apiClient = apiClient;
            _metadataProvider = metadataProvider;
        }

        public async Task<string> CreateBlogPostAsync(string topic, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic must be provided", nameof(topic));

            var input = topic.Trim();

            var agentDef = await _metadataProvider.GetAgentDefinitionAsync("Antigravity", cancellationToken);

            var result = await _apiClient.RunAgentWorkflowAsync(AgentName, Workflow, input, agentDef, cancellationToken);
            return result ?? string.Empty;
        }
    }
}
