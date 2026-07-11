using YamlDotNet.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

public class AgentDefinitionDto
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public List<string>? Responsibilities { get; set; }
    public string? Provider { get; set; }
    public string? AgentsMd { get; set; }

    /// <summary>Skill names from YAML 'skills' field - used to load actual skill definitions</summary>
    [YamlMember(Alias = "skills")]
    public List<string>? SkillNames { get; set; }

    /// <summary>Workflow names from YAML - used to load actual workflow definitions</summary>
    public List<string>? WorkflowNames { get; set; }

    /// <summary>Computed property: names extracted from loaded Skills</summary>
    public IEnumerable<string>? SkillsNames => Skills?.Select(s => s.Name);

    /// <summary>Computed property: names extracted from loaded Workflows</summary>
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
