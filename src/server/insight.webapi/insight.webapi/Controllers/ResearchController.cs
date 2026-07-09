using Insight.Services.Interfaces.Core;
using Insight.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace insight.webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchController(IResearchService researchService) : ControllerBase
{
    [HttpPost("ConductResearch")]
    public async Task<IActionResult> ConductResearch([FromBody] ConductResearchRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest("Topic is required.");

        var researchArtifacts = await researchService.ConductResearchAsync(request.Topic, request.Audience, request.WritingStyle, cancellationToken);
        return Ok(new ResearchResultsResponse { ResearchArtifacts = researchArtifacts });
    }
}
