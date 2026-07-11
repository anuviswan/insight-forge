using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgent(IGeminiApiClient apiClient, [FromKeyedServices("Gemini")] IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto> metadataProvider) : IBlogAgent, IAgentOrchestrator
{
    private const string AgentName = "Blog Writer Agent";
    private const string AgentId = "blog-writer-agent";

    public async Task<string> CheckIfAgentExists(string agentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("Agent ID must be provided", nameof(agentId));

        var exists = await apiClient.AgentExistsAsync(agentId, cancellationToken);
        return exists ? agentId : string.Empty;
    }

    public async Task<string> CreateAgent(string agentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("Agent ID must be provided", nameof(agentId));

        var agentDef = metadataProvider.GetAgent("Gemini");
        if (agentDef == null)
            throw new InvalidOperationException($"Agent definition not found for 'Gemini' provider");

        // Build system instruction from agent role and responsibilities
        var systemInstruction = BuildSystemInstruction(agentDef);
        var result = await apiClient.CreateManagedAgentAsync(agentId, systemInstruction, agentDef, cancellationToken);
        return result ?? agentId;
    }

    private static string BuildSystemInstruction(AgentDefinitionDto agentDef)
    {
        var instructionParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(agentDef.Name))
        {
            instructionParts.Add($"You are the {agentDef.Name}.");
        }

        if (!string.IsNullOrWhiteSpace(agentDef.Role))
        {
            instructionParts.Add($"Role: {agentDef.Role}");
        }

        if (agentDef.Responsibilities?.Any() == true)
        {
            instructionParts.Add("Responsibilities:");
            foreach (var responsibility in agentDef.Responsibilities)
            {
                instructionParts.Add($"- {responsibility}");
            }
        }

        if (!instructionParts.Any())
        {
            return $"You are the {AgentName} agent. Generate professional technical blog posts.";
        }

        return string.Join("\n", instructionParts);
    }

    public async Task<BlogEntry> CreateBlogPostAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        // Ensure managed agent exists
        var existsResult = await CheckIfAgentExists(AgentId, cancellationToken);
        if (string.IsNullOrEmpty(existsResult))
        {
            await CreateAgent(AgentId, cancellationToken);
        }

        var input = BuildBlogPrompt(topic, audience, writingStyle);

        var result = await apiClient.RunAgentInteractionAsync(AgentId, input, cancellationToken).ConfigureAwait(false);
        return new BlogEntry { Content = result ?? string.Empty };
    }

    private static string BuildBlogPrompt(string topic, string audience, string writingStyle)
    {
        var prompt = $"Write a comprehensive blog post about '{topic}'.";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"\n\nTarget Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nWriting Style: {writingStyle.Trim()}";

        prompt += "\n\nInclude citations, references, code examples where applicable, and ensure professional quality.";

        return prompt;
    }


}
