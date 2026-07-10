using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgent(IGeminiApiClient apiClient, IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto> metadataProvider) : IBlogAgent, IResearchAgent, IAgentOrchestrator
{
    private const string AgentName = "Blog Writer Agent";
    private const string BlogWorkflow = "create-blogpost-with-existing-research";
    private const string ResearchWorkflow = "research-only";

    public Task<string> CheckIfAgentExists(string agentName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateAgent(string agentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            throw new ArgumentException("Agent name must be provided", nameof(agentName));

        var agentDef = metadataProvider.GetAgent(agentName);
        if (agentDef == null)
            throw new InvalidOperationException($"Agent definition not found for '{agentName}'");

        var systemInstruction = agentDef.Content ?? $"You are the {agentName} agent.";
        var result = await apiClient.CreateManagedAgentAsync(agentName, systemInstruction, "", agentDef, cancellationToken);
        return result ?? string.Empty;
    }

    public async Task<string> CreateBlogPostAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        var input = BuildBlogPrompt(topic, audience, writingStyle, researchArtifacts: null);

        var agentDef = metadataProvider.GetAgent(AgentName);

        var result = await apiClient.RunAgentWorkflowAsync(AgentName, BlogWorkflow, input, agentDef, cancellationToken).ConfigureAwait(false);
        return result ?? string.Empty;
    }

    public async Task<string> CreateBlogPostWithResearchAsync(string topic, string audience, string writingStyle, string researchArtifacts, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        if (string.IsNullOrWhiteSpace(researchArtifacts))
            throw new ArgumentException("Research artifacts must be provided", nameof(researchArtifacts));

        var input = BuildBlogPrompt(topic, audience, writingStyle, researchArtifacts);

        var agentDef = metadataProvider.GetAgent(AgentName);

        var result = await apiClient.RunAgentWorkflowAsync(AgentName, BlogWorkflow, input, agentDef, cancellationToken).ConfigureAwait(false);
        return result ?? string.Empty;
    }

    public async Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        var input = BuildResearchPrompt(topic, audience, writingStyle);

        var agentDef = metadataProvider.GetAgent(AgentName);

        var result = await apiClient.RunAgentWorkflowAsync(AgentName, ResearchWorkflow, input, agentDef, cancellationToken).ConfigureAwait(false);
        return result ?? string.Empty;
    }

    private static string BuildBlogPrompt(string topic, string audience, string writingStyle, string? researchArtifacts)
    {
        var prompt = string.IsNullOrWhiteSpace(researchArtifacts)
            ? $"Create a comprehensive blog post about '{topic}'."
            : $"Create a professional blog post about '{topic}' using the provided research findings.\n\n" +
              $"Research Artifacts:\n{researchArtifacts}\n\n";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"Intended Audience: {audience.Trim()}\n\n";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"Writing Style/Tone: {writingStyle.Trim()}\n\n";

        prompt += "Structure Requirements:\n"
            + "1. Start with a clear, descriptive blog title\n"
            + "2. Include an Executive Summary (2-3 paragraphs) with high-level overview and key takeaways\n"
            + "3. Add a Table of Contents listing all major sections\n"
            + "4. Use appropriate section headings (H2, H3) to organize content logically\n"
            + "5. Include code snippets (with language tags) when demonstrating concepts or best practices\n"
            + "6. Use Mermaid diagrams to illustrate architecture, workflows, or conceptual relationships\n"
            + "7. Incorporate research findings with proper attribution throughout\n"
            + "8. End with References section citing all sources\n\n"
            + "Provide the complete blog post in well-formatted Markdown.";

        return prompt;
    }

    private static string BuildResearchPrompt(string topic, string audience, string writingStyle)
    {
        var prompt = $"Conduct comprehensive research on '{topic}'";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"\n\nIntended Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nWriting Style/Tone: {writingStyle.Trim()}";

        prompt += "\n\nProvide structured research artifacts including key findings, best practices, code samples, common pitfalls, and references.";

        return prompt;
    }
}
