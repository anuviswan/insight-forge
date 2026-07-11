using YamlDotNet.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

/// <summary>Agent definition with metadata and loaded resources for Gemini API.</summary>
public class AgentDefinitionDto
{
    /// <summary>Agent unique identifier from YAML 'id' field.</summary>
    [YamlMember(Alias = "id")]
    public string? Id { get; set; }

    /// <summary>Agent display name (used in system instruction).</summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>Agent role/purpose (used in system instruction).</summary>
    [YamlMember(Alias = "role")]
    public string? Role { get; set; }

    /// <summary>Agent responsibilities (used in system instruction).</summary>
    [YamlMember(Alias = "responsibilities")]
    public List<string>? Responsibilities { get; set; }

    /// <summary>Agent specification markdown (sent to Gemini as .agents/AGENTS.md).</summary>
    [YamlIgnore]
    public string? Specification { get; set; }

    /// <summary>Loaded workflow definitions (sent to Gemini as .agents/workflows/*.yaml).</summary>
    [YamlIgnore]
    public List<WorkflowDto>? Workflows { get; set; }

    /// <summary>Loaded skill definitions (sent to Gemini as .agents/skills/*/SKILL.md).</summary>
    [YamlIgnore]
    public List<SkillDto>? Skills { get; set; }

    /// <summary>Skill names from YAML 'skills' field (used during deserialization to load Skills).</summary>
    [YamlMember(Alias = "skills")]
    public List<string>? SkillNames { get; set; }

    /// <summary>Workflow names from YAML root 'workflows' field (used during deserialization to load Workflows).</summary>
    [YamlMember(Alias = "workflows")]
    public List<string>? WorkflowNames { get; set; }
}

/// <summary>Top-level YAML collection structure for agents and workflows.</summary>
public class AgentsCollectionDto
{
    /// <summary>List of agent definitions from YAML.</summary>
    public List<AgentDefinitionDto>? Agents { get; set; }

    /// <summary>List of workflow names from YAML root level.</summary>
    public List<string>? Workflows { get; set; }
}

/// <summary>Workflow definition with metadata and YAML content.</summary>
public class WorkflowDto
{
    /// <summary>Workflow name (used as identifier and in environment source target).</summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Workflow description from YAML.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    /// <summary>Workflow YAML content (sent to Gemini as .agents/workflows/{Name}.yaml).</summary>
    [YamlIgnore]
    public string Content { get; set; } = string.Empty;
}

/// <summary>Skill definition with metadata and YAML content.</summary>
public class SkillDto
{
    /// <summary>Skill name (used as identifier and in environment source target).</summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Skill description from YAML (used to build markdown content).</summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Skill instructions from YAML (used to build markdown content).</summary>
    [YamlMember(Alias = "instructions")]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Skill markdown content built from description and instructions (sent to Gemini as .agents/skills/{Name}/SKILL.md).</summary>
    [YamlIgnore]
    public string Content { get; set; } = string.Empty;
}
