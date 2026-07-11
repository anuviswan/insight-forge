using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Insight.Services.Ai.Gemini.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Insight.WebApi.Services;

public class MarkdownAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>
{
    private readonly string _agentRootFolder;
    private readonly string _workflowsDefinitionFolder;
    private readonly string _skillsDefinitionFolder;
    private readonly ILogger<MarkdownAgentMetadataProvider> _logger;
    private readonly IDeserializer _deserializer;
    private IDictionary<string, SkillDto>? _skillsCache;
    private IDictionary<string, WorkflowDto>? _workflowsCache;

    public MarkdownAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<MarkdownAgentMetadataProvider> logger)
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

        // Populate Skills from SkillNames (loaded from YAML/Markdown)
        if (dto.SkillNames?.Any() == true)
        {
            dto.Skills = new List<SkillDto>();
            foreach (var skillName in dto.SkillNames)
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

        // Populate Workflows from WorkflowNames (loaded from YAML/Markdown)
        if (dto.WorkflowNames?.Any() == true)
        {
            dto.Workflows = new List<WorkflowDto>();
            foreach (var workflowName in dto.WorkflowNames)
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

        // Prefer YAML files but fall back to markdown for backwards compatibility
        var agentsYaml = Path.Combine(root, "agents.yaml");
        var agentsYml = Path.Combine(root, "agents.yml");
        var agentsMd = Path.Combine(root, "agents.md");
        string? content = null;
        bool isYaml = false;

        if (File.Exists(agentsYaml))
        {
            content = await File.ReadAllTextAsync(agentsYaml, cancellationToken);
            isYaml = true;
        }
        else if (File.Exists(agentsYml))
        {
            content = await File.ReadAllTextAsync(agentsYml, cancellationToken);
            isYaml = true;
        }
        else if (File.Exists(agentsMd))
        {
            content = await File.ReadAllTextAsync(agentsMd, cancellationToken);
            isYaml = false;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("No agent definition found for {AgentIdentifier}", agentIdentifier);
            return dto;
        }

        try
        {
            List<AgentDefinitionDto> agents;
            if (isYaml)
            {
                agents = ExtractAgentsFromYaml(content);
            }
            else
            {
                agents = ExtractAgentsFromMarkdown(content);
            }

            if (agents.Any())
            {
                var agent = agents[0];
                agent.AgentsMd = content;
                agent.Workflows ??= new List<WorkflowDto>();
                agent.Skills ??= new List<SkillDto>();

                var wfDir = Path.Combine(root, "agents", "workflows");
                if (Directory.Exists(wfDir))
                {
                    var workflowFiles = Directory.EnumerateFiles(wfDir)
                        .Where(f => f.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yaml", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", System.StringComparison.OrdinalIgnoreCase))
                        .OrderBy(p => p);

                    foreach (var f in workflowFiles)
                    {
                        agent.Workflows.Add(new WorkflowDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
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
                        agent.Skills.Add(new SkillDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
                    }
                }

                PopulateSkillsAndWorkflows(agent);
                return agent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize agents for {AgentIdentifier}", agentIdentifier);
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
        var agentsMd = Path.Combine(root, "agents.md");

        string? content = null;
        bool isYaml = false;

        if (File.Exists(agentsYaml))
        {
            content = File.ReadAllText(agentsYaml);
            isYaml = true;
        }
        else if (File.Exists(agentsYml))
        {
            content = File.ReadAllText(agentsYml);
            isYaml = true;
        }
        else if (File.Exists(agentsMd))
        {
            content = File.ReadAllText(agentsMd);
            isYaml = false;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("No agents definition found for provider {Provider}", providerName);
            return;
        }

        try
        {
            List<AgentDefinitionDto> agents;
            if (isYaml)
            {
                agents = ExtractAgentsFromYaml(content);
            }
            else
            {
                agents = ExtractAgentsFromMarkdown(content);
            }

            foreach (var agent in agents)
            {
                agent.AgentsMd = content;

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
                        "Duplicate agent name '{AgentName}' in provider {Provider}",
                        key, providerName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load agents for provider {Provider}", providerName);
        }
    }

    private List<AgentDefinitionDto> ExtractAgentsFromYaml(string yamlText)
    {
        var agents = new List<AgentDefinitionDto>();
        try
        {
            var collection = _deserializer.Deserialize<AgentsCollectionDto>(yamlText);
            if (collection?.Agents != null)
            {
                foreach (var agent in collection.Agents)
                {
                    agent.Workflows ??= new List<WorkflowDto>();
                    agent.Skills ??= new List<SkillDto>();
                    agents.Add(agent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract YAML agents");
        }
        return agents;
    }

    private List<AgentDefinitionDto> ExtractAgentsFromMarkdown(string markdownText)
    {
        var agents = new List<AgentDefinitionDto>();
        try
        {
            var lines = markdownText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            AgentDefinitionDto? currentAgent = null;

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("### "))
                {
                    if (currentAgent != null)
                    {
                        agents.Add(currentAgent);
                    }

                    var agentName = line.Replace("###", "").Trim();
                    currentAgent = new AgentDefinitionDto
                    {
                        Name = agentName,
                        Workflows = new List<WorkflowDto>(),
                        Skills = new List<SkillDto>()
                    };
                }
            }

            if (currentAgent != null)
            {
                agents.Add(currentAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract markdown agents");
        }
        return agents;
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
    }
}
