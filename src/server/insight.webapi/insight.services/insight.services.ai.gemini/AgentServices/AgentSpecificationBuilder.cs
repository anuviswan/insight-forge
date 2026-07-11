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
        if (string.IsNullOrWhiteSpace(skill.Content))
            return "Specialized skill for the agent.";

        // Try to extract description from YAML frontmatter
        var lines = skill.Content.Split('\n');
        foreach (var line in lines)
        {
            if (line.StartsWith("description:"))
            {
                var desc = line.Replace("description:", "").Trim().Trim('"');
                return desc;
            }
        }

        // Fallback: first non-empty line after frontmatter
        var inFrontmatter = false;
        foreach (var line in lines)
        {
            if (line.StartsWith("---"))
            {
                inFrontmatter = !inFrontmatter;
                continue;
            }

            if (!inFrontmatter && !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                return line.Trim();
            }
        }

        return "Specialized skill for the agent.";
    }

    private static string ExtractWorkflowDescription(WorkflowDto workflow)
    {
        if (string.IsNullOrWhiteSpace(workflow.Content))
            return "Workflow for the agent.";

        // Try to extract description from YAML
        var lines = workflow.Content.Split('\n');
        foreach (var line in lines)
        {
            if (line.StartsWith("description:"))
            {
                var desc = line.Replace("description:", "").Trim().Trim('"', '\'');
                // Remove trailing quote if line ends with it
                if (desc.EndsWith("\"") || desc.EndsWith("'"))
                    desc = desc[..^1];
                return desc;
            }
        }

        return "Workflow for the agent.";
    }
}
