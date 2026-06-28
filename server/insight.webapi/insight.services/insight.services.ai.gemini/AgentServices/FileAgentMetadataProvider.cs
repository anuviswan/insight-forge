using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Insight.WebApi.Services;

public class MarkdownAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>
{
    private readonly string _agentRootFolder;
    private readonly ILogger<MarkdownAgentMetadataProvider> _logger;

    public MarkdownAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<MarkdownAgentMetadataProvider> logger)
    {
        _agentRootFolder = options?.Value?.AgentRootFolder ?? "agents";
        _logger = logger;
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

        return dto;
    }

    public SkillDto GetSkill(string skillName)
    {
        // Search all agents for the skill
        if (!Directory.Exists(_agentRootFolder)) return new SkillDto { Name = skillName };

        foreach (var agentDir in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var skillsDir = Path.Combine(agentDir, "agents", "skills");
            if (!Directory.Exists(skillsDir)) continue;

            var match = Directory.EnumerateFiles(skillsDir)
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), skillName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return new SkillDto { Name = skillName, Content = File.ReadAllText(match) };
            }
        }

        return new SkillDto { Name = skillName };
    }

    public WorkflowDto GetWorkflow(string workflowName)
    {
        // Search all agents for the workflow
        if (!Directory.Exists(_agentRootFolder)) return new WorkflowDto { Name = workflowName };

        foreach (var agentDir in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var wfDir = Path.Combine(agentDir, "agents", "workflows");
            if (!Directory.Exists(wfDir)) continue;

            var match = Directory.EnumerateFiles(wfDir)
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), workflowName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return new WorkflowDto { Name = workflowName, Content = File.ReadAllText(match) };
            }
        }

        return new WorkflowDto { Name = workflowName };
    }

    public IDictionary<string, AgentDefinitionDto> Load(string agentFolder)
    {
        var result = new Dictionary<string, AgentDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        var folder = Path.Combine(_agentRootFolder, agentFolder);
        if (!Directory.Exists(folder)) return result;

        foreach (var sub in Directory.EnumerateDirectories(folder))
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

    IDictionary<string, SkillDto> ICanReadSkillDefinition<SkillDto>.Load(string skillFolder)
    {
        var result = new Dictionary<string, SkillDto>(StringComparer.OrdinalIgnoreCase);
        var folder = Path.Combine(_agentRootFolder, skillFolder);
        if (!Directory.Exists(folder)) return result;

        var files = Directory.EnumerateFiles(folder)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

        foreach (var f in files)
        {
            var name = Path.GetFileNameWithoutExtension(f);
            result[name] = new SkillDto { Name = name, Content = File.ReadAllText(f) };
        }

        return result;
    }

    IDictionary<string, WorkflowDto> ICanReadWorkflowDefinition<WorkflowDto>.Load(string workflowFolder)
    {
        var result = new Dictionary<string, WorkflowDto>(StringComparer.OrdinalIgnoreCase);
        var folder = Path.Combine(_agentRootFolder, workflowFolder);
        if (!Directory.Exists(folder)) return result;

        var files = Directory.EnumerateFiles(folder)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

        foreach (var f in files)
        {
            var name = Path.GetFileNameWithoutExtension(f);
            result[name] = new WorkflowDto { Name = name, Content = File.ReadAllText(f) };
        }

        return result;
    }
}
