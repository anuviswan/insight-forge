namespace Insight.Services.Ai.Gemini.Types;

public class AgentDefinitionDto
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public List<string>? Responsibilities { get; set; }
    public string? Provider { get; set; }
    public string? AgentsMd { get; set; }

    public IEnumerable<string>? SkillsNames => Skills?.Select(s => s.Name);
    public IEnumerable<string>? WorkflowsNames => Workflows?.Select(w => w.Name);

    public List<WorkflowDto>? Workflows { get; set; }
    public List<SkillDto>? Skills { get; set; }

    public string Content { get; set; } = string.Empty;
}

public class AgentsCollectionDto
{
    public string? Description { get; set; }
    public List<AgentDefinitionDto>? Agents { get; set; }
    public List<string>? Workflows { get; set; }
    public List<string>? OutputFolders { get; set; }
}

public class WorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SkillDto
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
