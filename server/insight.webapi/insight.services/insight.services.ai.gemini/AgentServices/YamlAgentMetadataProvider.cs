using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Ai.Gemini.AgentServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Insight.WebApi.Services;

public class YamlAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>
{
    private readonly string _agentRootFolder;
    private readonly ILogger<YamlAgentMetadataProvider> _logger;
    private readonly IDeserializer _deserializer;
    private IDictionary<string, SkillDto>? _skillsCache;
    private IDictionary<string, WorkflowDto>? _workflowsCache;

    public YamlAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<YamlAgentMetadataProvider> logger)
    {
        _agentRootFolder = options?.Value?.AgentsDefinitionFile ?? "agents";
        _logger = logger;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    private void PopulateSkillsAndWorkflows(AgentDefinitionDto dto)
    {
        // Lazy-load cached dictionaries if not already loaded
        _skillsCache ??= LoadSkills();
        _workflowsCache ??= LoadWorkflow();

        // Populate Skills from names using the cached dictionary
        if (dto.SkillsNames?.Any() == true)
        {
            dto.Skills = new List<SkillDto>();
            foreach (var skillName in dto.SkillsNames)
            {
                if (_skillsCache.TryGetValue(skillName, out var skillDto))
                {
                    dto.Skills.Add(skillDto);
                }
                else
                {
                    _logger.LogWarning("Skill '{SkillName}' referenced but not found in definitions", skillName);
                    dto.Skills.Add(new SkillDto { Name = skillName });
                }
            }
        }

        // Populate Workflows from names using the cached dictionary
        if (dto.WorkflowsNames?.Any() == true)
        {
            dto.Workflows = new List<WorkflowDto>();
            foreach (var workflowName in dto.WorkflowsNames)
            {
                if (_workflowsCache.TryGetValue(workflowName, out var workflowDto))
                {
                    dto.Workflows.Add(workflowDto);
                }
                else
                {
                    _logger.LogWarning("Workflow '{WorkflowName}' referenced but not found in definitions", workflowName);
                    dto.Workflows.Add(new WorkflowDto { Name = workflowName });
                }
            }
        }

        // Ensure lists are initialized even if empty
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

        var agentsYaml = Path.Combine(root, "agents.yaml");
        var agentsYml = Path.Combine(root, "agents.yml");

        string? yamlText = null;
        if (File.Exists(agentsYaml)) yamlText = await File.ReadAllTextAsync(agentsYaml, cancellationToken);
        else if (File.Exists(agentsYml)) yamlText = await File.ReadAllTextAsync(agentsYml, cancellationToken);

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _logger.LogWarning("No structured YAML agent definition found for {AgentFolder}", agentFolder);
            return dto;
        }

        try
        {
            var mapped = _deserializer.Deserialize<AgentDefinitionDto>(yamlText!);
            if (mapped != null)
            {
                mapped.Content = yamlText!;
                mapped.Workflows ??= new List<WorkflowDto>();
                mapped.Skills ??= new List<SkillDto>();

                // Populate skills and workflows from cached dictionaries based on names
                PopulateSkillsAndWorkflows(mapped);

                return mapped;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize YAML for agent {AgentFolder}", agentFolder);
        }

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
        if (!Directory.Exists(_agentRootFolder)) return result;

        foreach (var agentDir in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var agentsYaml = Path.Combine(agentDir, "agents.yaml");
            var agentsYml = Path.Combine(agentDir, "agents.yml");
            string? yamlText = null;
            if (File.Exists(agentsYaml)) yamlText = File.ReadAllText(agentsYaml);
            else if (File.Exists(agentsYml)) yamlText = File.ReadAllText(agentsYml);

            if (string.IsNullOrWhiteSpace(yamlText)) continue;

            try
            {
                var mapped = _deserializer.Deserialize<AgentDefinitionDto>(yamlText);
                if (mapped?.Skills != null)
                {
                    foreach (var s in mapped.Skills)
                    {
                        if (!result.ContainsKey(s.Name)) result[s.Name] = s;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "YAML parse failed when loading skills from agent {Dir}", agentDir);
            }
        }

        return result;
    }

    public IDictionary<string, WorkflowDto> LoadWorkflow()
    {
        var result = new Dictionary<string, WorkflowDto>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(_agentRootFolder)) return result;

        foreach (var agentDir in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var agentsYaml = Path.Combine(agentDir, "agents.yaml");
            var agentsYml = Path.Combine(agentDir, "agents.yml");
            string? yamlText = null;
            if (File.Exists(agentsYaml)) yamlText = File.ReadAllText(agentsYaml);
            else if (File.Exists(agentsYml)) yamlText = File.ReadAllText(agentsYml);

            if (string.IsNullOrWhiteSpace(yamlText)) continue;

            try
            {
                var mapped = _deserializer.Deserialize<AgentDefinitionDto>(yamlText);
                if (mapped?.Workflows != null)
                {
                    foreach (var w in mapped.Workflows)
                    {
                        if (!result.ContainsKey(w.Name)) result[w.Name] = w;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "YAML parse failed when loading workflows from agent {Dir}", agentDir);
            }
        }

        return result;
    }
}
