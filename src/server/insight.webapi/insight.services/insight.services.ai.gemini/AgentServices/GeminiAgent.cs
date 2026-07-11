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

        var agentDef = metadataProvider.GetAgent(AgentName);
        if (agentDef == null)
            throw new InvalidOperationException($"Agent definition not found for '{AgentName}'");

        var systemInstruction = agentDef.Content ?? $"You are the {AgentName} agent. Execute the create-blogpost workflow to generate professional technical blog posts with research integration, domain analysis, and content quality optimization.";
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
            prompt += $"\n\nIntended Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nWriting Style/Tone: {writingStyle.Trim()}";

        prompt += "\n\nIMPORTANT: Include citations and references in your blog post:\n"
                + "- Use markdown links [text](url) for citations throughout the content\n"
                + "- Include a ## References section at the end with a bulleted list of sources\n"
                + "- Ensure the blog post is well-structured with proper headings\n"
                + "Provide the complete blog post in well-formatted Markdown.";

        return prompt;
    }

    private static string BuildResearchPrompt(string topic, string audience, string writingStyle)
    {
        var prompt = $"Conduct thorough research on the topic: '{topic}'.";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"\n\nTarget Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nContext: {writingStyle.Trim()}";

        prompt += "\n\nProvide research findings in a structured format with:\n"
                + "- Key findings and insights\n"
                + "- Relevant sources and references\n"
                + "- Data points and statistics\n"
                + "- Current trends and developments\n"
                + "Format the results as a comprehensive research summary.";

        return prompt;
    }

}
