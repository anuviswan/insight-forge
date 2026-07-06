using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Insight.Services.Ai.Gemini.Options;

namespace Insight.WebApi.Services;

public class YamlAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>
{
    private readonly string _agentRootFolder;
    private readonly string _workflowsDefinitionFolder;
    private readonly string _skillsDefinitionFolder;
    private readonly ILogger<YamlAgentMetadataProvider> _logger;
    private readonly IDeserializer _deserializer;
    private IDictionary<string, SkillDto>? _skillsCache;
    private IDictionary<string, WorkflowDto>? _workflowsCache;

    public YamlAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<YamlAgentMetadataProvider> logger)
    {
        _agentRootFolder = options?.Value?.AgentsDefinitionFile ?? "agents";
        _workflowsDefinitionFolder = options?.Value?.WorkflowsDefinitionFolder ?? "workflows";
        _skillsDefinitionFolder = options?.Value?.SkillsDefinitionFolder ?? "skills";
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

    public async Task<AgentDefinitionDto> GetAgentDefinitionAsync(string agentIdentifier, CancellationToken cancellationToken = default)
    {
        var root = Path.Combine(_agentRootFolder, agentIdentifier);
        var dto = new AgentDefinitionDto
        {
            Workflows = new List<WorkflowDto>(),
            Skills = new List<SkillDto>()
        };

        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Agent or provider folder not found: {Path}", root);
            return dto;
        }

        var agentsYaml = Path.Combine(root, "agents.yaml");
        var agentsYml = Path.Combine(root, "agents.yml");

        string? yamlText = null;
        if (File.Exists(agentsYaml)) yamlText = await File.ReadAllTextAsync(agentsYaml, cancellationToken);
        else if (File.Exists(agentsYml)) yamlText = await File.ReadAllTextAsync(agentsYml, cancellationToken);

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _logger.LogWarning("No structured YAML agent definition found for {AgentIdentifier}", agentIdentifier);
            return dto;
        }

        try
        {
            // Try to deserialize as collection first (new pattern)
            var collection = _deserializer.Deserialize<AgentsCollectionDto>(yamlText);
            if (collection?.Agents?.Any() == true)
            {
                var agent = collection.Agents[0];
                agent.Provider = agentIdentifier;
                agent.Content = yamlText;
                agent.Workflows ??= new List<WorkflowDto>();
                agent.Skills ??= new List<SkillDto>();
                PopulateSkillsAndWorkflows(agent);
                return agent;
            }

            // Fallback: try direct deserialization (backward compatibility)
            var mapped = _deserializer.Deserialize<AgentDefinitionDto>(yamlText);
            if (mapped != null)
            {
                mapped.Content = yamlText;
                mapped.Workflows ??= new List<WorkflowDto>();
                mapped.Skills ??= new List<SkillDto>();
                PopulateSkillsAndWorkflows(mapped);
                return mapped;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize YAML for {AgentIdentifier}", agentIdentifier);
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

        foreach (var providerFolder in Directory.EnumerateDirectories(_agentRootFolder))
        {
            var providerName = Path.GetFileName(providerFolder);
            LoadAgentsFromProvider(providerName, result);
        }

        return result;
    }

    private void LoadAgentsFromProvider(string providerName, IDictionary<string, AgentDefinitionDto> result)
    {
        var root = Path.Combine(_agentRootFolder, providerName);
        var agentsYaml = Path.Combine(root, "agents.yaml");
        var agentsYml = Path.Combine(root, "agents.yml");

        string? yamlText = null;
        if (File.Exists(agentsYaml)) yamlText = File.ReadAllText(agentsYaml);
        else if (File.Exists(agentsYml)) yamlText = File.ReadAllText(agentsYml);

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _logger.LogWarning("No agents definition found for provider {Provider}", providerName);
            return;
        }

        try
        {
            var collection = _deserializer.Deserialize<AgentsCollectionDto>(yamlText);
            if (collection?.Agents?.Any() != true)
            {
                _logger.LogWarning("No agents found in {Provider}/agents.yaml", providerName);
                return;
            }

            foreach (var agent in collection.Agents)
            {
                agent.Provider = providerName;
                agent.Content = yamlText;

                string key = !string.IsNullOrWhiteSpace(agent.Name)
                    ? agent.Name
                    : providerName;

                if (!result.ContainsKey(key))
                {
                    PopulateSkillsAndWorkflows(agent);
                    result[key] = agent;
                }
                else
                {
                    _logger.LogWarning(
                        "Duplicate agent name '{AgentName}' in provider {Provider}. Skipping.",
                        key, providerName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize agents for provider {Provider}", providerName);
        }
    }

    public IDictionary<string, SkillDto> LoadSkills()
    {
        var result = new Dictionary<string, SkillDto>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_skillsDefinitionFolder))
        {
            _logger.LogWarning("Skills definition folder not found: {Path}", _skillsDefinitionFolder);
            return result;
        }

        // Get all YAML files from the skills definition folder
        var skillFiles = Directory.EnumerateFiles(_skillsDefinitionFolder)
            .Where(f => f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p);

        foreach (var skillFile in skillFiles)
        {
            try
            {
                var yamlText = File.ReadAllText(skillFile);
                if (string.IsNullOrWhiteSpace(yamlText)) continue;

                var skill = _deserializer.Deserialize<SkillDto>(yamlText);
                if (skill != null && !string.IsNullOrWhiteSpace(skill.Name))
                {
                    skill.Content = yamlText;
                    result[skill.Name] = skill;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize skill from {FilePath}", skillFile);
            }
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

        // Get all YAML files from the workflows definition folder
        var workflowFiles = Directory.EnumerateFiles(_workflowsDefinitionFolder)
            .Where(f => f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p);

        foreach (var workflowFile in workflowFiles)
        {
            try
            {
                var yamlText = File.ReadAllText(workflowFile);
                if (string.IsNullOrWhiteSpace(yamlText)) continue;

                var workflow = _deserializer.Deserialize<WorkflowDto>(yamlText);
                if (workflow != null && !string.IsNullOrWhiteSpace(workflow.Name))
                {
                    workflow.Content = yamlText;
                    result[workflow.Name] = workflow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize workflow from {FilePath}", workflowFile);
            }
        }

        return result;
    }
}
