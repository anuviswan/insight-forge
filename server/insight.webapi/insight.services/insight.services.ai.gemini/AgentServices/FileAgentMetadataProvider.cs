using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Insight.WebApi.Services;

public class FileAgentMetadataProvider : IAgentMetadataProvider<AgentDefinitionDto>
{
    private readonly string _agentRootFolder;
    private readonly ILogger<FileAgentMetadataProvider> _logger;

    public FileAgentMetadataProvider([FromKeyedServices("Gemini")]string agentFolder, ILogger<FileAgentMetadataProvider> logger)
    {
        _agentRootFolder = agentFolder;
        _logger = logger;
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
}
