using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgent(
    IGeminiApiClient apiClient,
    [FromKeyedServices("Gemini")] IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto> metadataProvider,
    ILoggerFactory loggerFactory) : IBlogAgent, IAgentOrchestrator
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

        // Validate all required components are present
        ValidateAgentDefinition(agentDef);

        // Build system instruction from agent role and responsibilities
        var systemInstruction = BuildSystemInstruction(agentDef);
        var result = await apiClient.CreateManagedAgentAsync(agentId, systemInstruction, agentDef, cancellationToken);
        return result ?? agentId;
    }

    private static void ValidateAgentDefinition(AgentDefinitionDto agentDef)
    {
        var missingComponents = new List<string>();

        if (string.IsNullOrWhiteSpace(agentDef.Specification))
            missingComponents.Add("Specification (agent specification)");

        if (agentDef.Workflows == null || agentDef.Workflows.Count == 0)
            missingComponents.Add("Workflows (0 workflows loaded)");
        else
        {
            foreach (var wf in agentDef.Workflows)
            {
                if (string.IsNullOrWhiteSpace(wf.Content))
                    missingComponents.Add($"Workflow '{wf.Name}' has empty content");
            }
        }

        if (agentDef.Skills == null || agentDef.Skills.Count == 0)
            missingComponents.Add("Skills (0 skills loaded)");
        else
        {
            foreach (var skill in agentDef.Skills)
            {
                if (string.IsNullOrWhiteSpace(skill.Content))
                    missingComponents.Add($"Skill '{skill.Name}' has empty content");
            }
        }

        if (missingComponents.Any())
        {
            var message = $"Cannot create agent - missing or invalid components:\n  - " +
                         string.Join("\n  - ", missingComponents);
            throw new InvalidOperationException(message);
        }
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

    public async Task<BlogEntry> CreateBlogPostStreamedAsync(string topic, string audience, string writingStyle, IEventBus eventBus, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
        ArgumentNullException.ThrowIfNull(eventBus, nameof(eventBus));

        // Ensure managed agent exists
        var existsResult = await CheckIfAgentExists(AgentId, cancellationToken);
        if (string.IsNullOrEmpty(existsResult))
        {
            await CreateAgent(AgentId, cancellationToken);
        }

        var input = BuildBlogPrompt(topic, audience, writingStyle);
        var transformerLogger = loggerFactory.CreateLogger<GeminiEventTransformer>();
        var transformer = new GeminiEventTransformer(transformerLogger);
        var accumulatedContent = new System.Text.StringBuilder();

        // Note: the event bus is a job-scoped resource owned by the caller (via
        // IJobAgentService). It is intentionally left open here - even if streaming
        // throws, the caller still needs to publish an Error event and complete the
        // job afterwards. Completing it here would drop that final event.
        await foreach (var geminiEvent in apiClient.StreamAgentInteractionAsync(AgentId, input, cancellationToken))
        {
            var events = transformer.Transform(geminiEvent);
            foreach (var @event in events)
            {
                await eventBus.PublishAsync(@event, cancellationToken);

                // Each StepCompleted event carries only the text produced by that
                // single step (the transformer resets its accumulator per step), so
                // final content must be summed across model_output steps rather than
                // overwritten - a later non-text step (e.g. a tool call) would
                // otherwise wipe out text already collected from an earlier step.
                if (@event.EventType == AgentEventType.StepCompleted
                    && @event.Data?.TryGetValue("step_type", out var stepType) == true
                    && string.Equals(stepType as string, "model_output", StringComparison.Ordinal)
                    && @event.Data.TryGetValue("final_content", out var finalContent)
                    && finalContent is string stepContent
                    && !string.IsNullOrEmpty(stepContent))
                {
                    accumulatedContent.Append(stepContent);
                }
            }
        }

        return new BlogEntry { Content = accumulatedContent.ToString() };
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
