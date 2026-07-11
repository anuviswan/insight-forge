using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgent(IGeminiApiClient apiClient, IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto> metadataProvider) : IBlogAgent, IResearchAgent, IAgentOrchestrator
{
    private const string AgentName = "Blog Writer Agent";
    private const string AgentId = "blog-writer-agent";
    private const string ResearchAgentId = "research-agent";

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

        Console.WriteLine($"[DEBUG] Agent Definition Loaded:");
        Console.WriteLine($"  - Name: {agentDef.Name}");
        Console.WriteLine($"  - Has AgentsMd: {!string.IsNullOrWhiteSpace(agentDef.AgentsMd)}");
        Console.WriteLine($"  - Workflows: {agentDef.Workflows?.Count ?? 0}");
        Console.WriteLine($"  - Skills: {agentDef.Skills?.Count ?? 0}");
        if (agentDef.Skills?.Count > 0)
        {
            Console.WriteLine($"  - Skill Names: {string.Join(", ", agentDef.Skills.Select(s => s.Name))}");
        }

        var systemInstruction = agentDef.Content ?? $"You are the {AgentName} agent. Generate professional technical blog posts with research integration, domain analysis, and content quality optimization.";
        var result = await apiClient.CreateManagedAgentAsync(agentId, systemInstruction, agentDef, cancellationToken);
        return result ?? agentId;
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

    public async Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        // Ensure research agent exists
        var existsResult = await CheckIfAgentExists(ResearchAgentId, cancellationToken);
        if (string.IsNullOrEmpty(existsResult))
        {
            await CreateAgent(ResearchAgentId, cancellationToken);
        }

        var input = BuildResearchPrompt(topic, audience, writingStyle);

        var result = await apiClient.RunAgentInteractionAsync(ResearchAgentId, input, cancellationToken).ConfigureAwait(false);
        return result ?? string.Empty;
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

    private static string BuildResearchPrompt(string topic, string audience, string writingStyle)
    {
        var prompt = $"Conduct thorough research on the topic: '{topic}'.";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"\n\nTarget Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nContext: {writingStyle.Trim()}";

        prompt += "\n\nProvide research findings in a structured format with key findings, sources, and data points.";

        return prompt;
    }

}
