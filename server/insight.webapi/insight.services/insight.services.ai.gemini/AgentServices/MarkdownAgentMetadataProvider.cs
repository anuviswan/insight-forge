using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Insight.WebApi.Services;

public class MarkdownAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>
{
    private readonly string _agentRootFolder;
    private readonly string _workflowsDefinitionFolder;
    private readonly string _skillsDefinitionFolder;
    private readonly ILogger<MarkdownAgentMetadataProvider> _logger;
    private IDictionary<string, SkillDto>? _skillsCache;
    private IDictionary<string, WorkflowDto>? _workflowsCache;

    public MarkdownAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<MarkdownAgentMetadataProvider> logger)
    {
        _agentRootFolder = options?.Value?.AgentsDefinitionFile ?? "agents";
        _workflowsDefinitionFolder = options?.Value?.WorkflowsDefinitionFolder ?? "workflows";
        _skillsDefinitionFolder = options?.Value?.SkillsDefinitionFolder ?? "skills";
        _logger = logger;
    }

    private void PopulateSkillsAndWorkflows(AgentDefinitionDto dto)
    {
        // Lazy-load cached dictionaries if not already loaded
        _skillsCache ??= LoadSkills();
        _workflowsCache ??= LoadWorkflow();

        // Ensure lists are initialized
        dto.Skills ??= new List<SkillDto>();
        dto.Workflows ??= new List<WorkflowDto>();
    }

    public AgentDefinitionDto GetAgent(string agentName)
    {
        return GetAgentDefinitionAsync(agentName).GetAwaiter().GetResult();
    }

    public async Task<AgentDefinitionDto> GetAgentDefinitionAsync(string agentFolder, CancellationToken cancellationToken = default)
    {
        var root = Path.Combine(_agentRootFolder, agentFolder);
        var dto = new AgentDefinitionDto
        {
            Workflows = new List<WorkflowDto>(),
            Skills = new List<SkillDto>()
        };

        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Agent folder not found: {Path}", root);
            return dto;
        }

        // Prefer YAML files but fall back to markdown for backwards compatibility
        var agentsYaml = Path.Combine(root, "agents.yaml");
        var agentsYml = Path.Combine(root, "agents.yml");
        var agentsMd = Path.Combine(root, "agents.md");
        if (File.Exists(agentsYaml))
        {
            dto.AgentsMd = await File.ReadAllTextAsync(agentsYaml, cancellationToken);
        }
        else if (File.Exists(agentsYml))
        {
            dto.AgentsMd = await File.ReadAllTextAsync(agentsYml, cancellationToken);
        }
        else if (File.Exists(agentsMd))
        {
            dto.AgentsMd = await File.ReadAllTextAsync(agentsMd, cancellationToken);
        }

        var wfDir = Path.Combine(root, "agents", "workflows");
        if (Directory.Exists(wfDir))
        {
            var workflowFiles = Directory.EnumerateFiles(wfDir)
                .Where(f => f.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yaml", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p);

            foreach (var f in workflowFiles)
            {
                dto.Workflows.Add(new WorkflowDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
            }
        }

        var skillsDir = Path.Combine(root, "agents", "skills");
        if (Directory.Exists(skillsDir))
        {
            var skillFiles = Directory.EnumerateFiles(skillsDir)
                .Where(f => f.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yaml", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p);

            foreach (var f in skillFiles)
            {
                dto.Skills.Add(new SkillDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
            }
        }

        // Populate skills and workflows using helper
        PopulateSkillsAndWorkflows(dto);

        return dto;
    }

    public SkillDto GetSkill(string skillName)
    {
        // Use LoadSkills to get all skills with full definitions
        var skills = _skillsCache ??= LoadSkills();
        if (skills.TryGetValue(skillName, out var skill))
            return skill;

        _logger.LogWarning("Skill '{SkillName}' not found in definitions", skillName);
        return new SkillDto { Name = skillName };
    }

    public WorkflowDto GetWorkflow(string workflowName)
    {
        // Use LoadWorkflow to get all workflows with full definitions
        var workflows = _workflowsCache ??= LoadWorkflow();
        if (workflows.TryGetValue(workflowName, out var workflow))
            return workflow;

        _logger.LogWarning("Workflow '{WorkflowName}' not found in definitions", workflowName);
        return new WorkflowDto { Name = workflowName };
    }

    public IDictionary<string, AgentDefinitionDto> LoadAgents()
    {
        var result = new Dictionary<string, AgentDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(_agentRootFolder)) return result;

        foreach (var sub in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var name = Path.GetFileName(sub);
            try
            {
                var dto = GetAgentDefinitionAsync(name).GetAwaiter().GetResult();
                result[name] = dto;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load agent {Agent}", name);
            }
        }

        return result;
    }

    public IDictionary<string, SkillDto> LoadSkills()
    {
        var result = new Dictionary<string, SkillDto>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_skillsDefinitionFolder))
        {
            _logger.LogWarning("Skills definition folder not found: {Path}", _skillsDefinitionFolder);
            return result;
        }

        // Get all markdown and YAML files from the skills definition folder
        var files = Directory.EnumerateFiles(_skillsDefinitionFolder)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

        foreach (var f in files)
        {
            var name = Path.GetFileNameWithoutExtension(f);
            result[name] = new SkillDto { Name = name, Content = File.ReadAllText(f) };
        }

        return result;
    }

    public IDictionary<string, WorkflowDto> LoadWorkflow()
    {
        var result = new Dictionary<string, WorkflowDto>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_workflowsDefinitionFolder))
        {
            _logger.LogWarning("Workflows definition folder not found: {Path}", _workflowsDefinitionFolder);
            return result;
        }

        // Get all markdown and YAML files from the workflows definition folder
        var files = Directory.EnumerateFiles(_workflowsDefinitionFolder)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

        foreach (var f in files)
        {
            var name = Path.GetFileNameWithoutExtension(f);
            result[name] = new WorkflowDto { Name = name, Content = File.ReadAllText(f) };
        }

        return result;

        return result;
    }
}
