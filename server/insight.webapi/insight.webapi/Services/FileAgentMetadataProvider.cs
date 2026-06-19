using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace insight.webapi.Services
{
    public class FileAgentMetadataProvider : IAgentMetadataProvider
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileAgentMetadataProvider> _logger;

        public FileAgentMetadataProvider(IWebHostEnvironment env, ILogger<FileAgentMetadataProvider> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<AgentDefinitionDto> GetAgentDefinitionAsync(string agentFolder, CancellationToken cancellationToken = default)
        {
            var root = Path.Combine(_env.ContentRootPath, "Agents", agentFolder);
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
}
