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

    public YamlAgentMetadataProvider(IOptions<GeminiAgentOptions> options, ILogger<YamlAgentMetadataProvider> logger)
    {
        _agentRootFolder = options?.Value?.AgentRootFolder ?? "agents";
        _logger = logger;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
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

        // Require a single agents.yaml/yml file containing structured metadata
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
            // Map YAML into AgentDefinitionDto (structured)
            var mapped = _deserializer.Deserialize<AgentDefinitionDto>(yamlText!);
            if (mapped != null)
            {
                // preserve raw content too
                mapped.Content = yamlText!;
                // ensure lists are not null
                mapped.Workflows ??= new List<WorkflowDto>();
                mapped.Skills ??= new List<SkillDto>();
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
        // Search all agents for the skill (YAML only)
        if (!Directory.Exists(_agentRootFolder)) return new SkillDto { Name = skillName };

        foreach (var agentDir in Directory.EnumerateDirectories(_agentRootFolder))
        {
            // Check for a structured YAML agent file
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
                    var s = mapped.Skills.FirstOrDefault(sk => string.Equals(sk.Name, skillName, StringComparison.OrdinalIgnoreCase));
                    if (s != null) return s;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "YAML parse failed when searching for skill {Skill} in agent {Dir}", skillName, agentDir);
            }
        }

        return new SkillDto { Name = skillName };
    }

    public WorkflowDto GetWorkflow(string workflowName)
    {
        // Search all agents for the workflow (YAML only)
        if (!Directory.Exists(_agentRootFolder)) return new WorkflowDto { Name = workflowName };

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
                    var w = mapped.Workflows.FirstOrDefault(wf => string.Equals(wf.Name, workflowName, StringComparison.OrdinalIgnoreCase));
                    if (w != null) return w;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "YAML parse failed when searching for workflow {Workflow} in agent {Dir}", workflowName, agentDir);
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
            .Where(f => f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

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
            .Where(f => f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));

        foreach (var f in files)
        {
            var name = Path.GetFileNameWithoutExtension(f);
            result[name] = new WorkflowDto { Name = name, Content = File.ReadAllText(f) };
        }

        return result;
    }
}
