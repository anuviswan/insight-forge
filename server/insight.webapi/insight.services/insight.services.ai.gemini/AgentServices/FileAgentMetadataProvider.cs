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
            Workflows = [],
            Skills = []
        };

        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Agent folder not found: {Path}", root);
            return dto;
        }

        var agentsMd = Path.Combine(root, "agents.md");
        if (File.Exists(agentsMd))
        {
            dto.AgentsMd = await File.ReadAllTextAsync(agentsMd, cancellationToken);
        }

        var wfDir = Path.Combine(root, "agents", "workflows");
        if (Directory.Exists(wfDir))
        {
            foreach (var f in Directory.EnumerateFiles(wfDir, "*.md").OrderBy(p => p))
            {
                dto.Workflows.Add(new WorkflowDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
            }
        }

        var skillsDir = Path.Combine(root, "agents", "skills");
        if (Directory.Exists(skillsDir))
        {
            foreach (var f in Directory.EnumerateFiles(skillsDir, "*.md").OrderBy(p => p))
            {
                dto.Skills.Add(new SkillDto { Name = Path.GetFileNameWithoutExtension(f), Content = await File.ReadAllTextAsync(f, cancellationToken) });
            }
        }

        return dto;
    }
}
