using Insight.Services.Ai.Gemini.Types;

namespace Insight.WebApi.Services;

/// <summary>Builds agent specification markdown from deserialized agent data.</summary>
public static class AgentSpecificationBuilder
{
    /// <summary>Build specification text from agent definition data.</summary>
    public static string BuildSpecification(AgentDefinitionDto agent, IDictionary<string, SkillDto>? skillsCache = null)
    {
        var lines = new List<string>();

        // Agent name and role
        if (!string.IsNullOrWhiteSpace(agent.Name))
        {
            lines.Add($"# {agent.Name}");
            lines.Add("");
        }

        if (!string.IsNullOrWhiteSpace(agent.Role))
        {
            lines.Add($"**Role:** {agent.Role}");
            lines.Add("");
        }

        // Responsibilities
        if (agent.Responsibilities?.Any() == true)
        {
            lines.Add("## Responsibilities");
            lines.Add("");
            foreach (var responsibility in agent.Responsibilities)
            {
                lines.Add($"- {responsibility}");
            }
            lines.Add("");
        }

        // Available skills with descriptions
        if (agent.Skills?.Any() == true)
        {
            lines.Add("## Available Skills");
            lines.Add("");
            foreach (var skill in agent.Skills)
            {
                lines.Add($"- **{skill.Name}** - {ExtractSkillDescription(skill)}");
            }
            lines.Add("");
        }

        // Available workflows
        if (agent.Workflows?.Any() == true)
        {
            lines.Add("## Available Workflows");
            lines.Add("");
            foreach (var workflow in agent.Workflows)
            {
                lines.Add($"- **{workflow.Name}** - {ExtractWorkflowDescription(workflow)}");
            }
            lines.Add("");
        }

        return string.Join("\n", lines).Trim();
    }

    private static string ExtractSkillDescription(SkillDto skill)
    {
        // Use description field if available
        if (!string.IsNullOrWhiteSpace(skill.Description))
            return skill.Description;

        return "Specialized skill for the agent.";
    }

    private static string ExtractWorkflowDescription(WorkflowDto workflow)
    {
        // Use description field if available
        if (!string.IsNullOrWhiteSpace(workflow.Description))
            return workflow.Description;

        return "Workflow for the agent.";
    }
}
