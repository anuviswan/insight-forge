namespace Insight.Services.Ai.Gemini.Types;

public class AgentDefinitionDto
{
    public string? AgentsMd { get; set; }
    public List<WorkflowDto>? Workflows { get; set; }
    public List<SkillDto>? Skills { get; set; }
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
