using Insight.Services.Interfaces.Core;
using Insight.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace insight.webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchController(IResearchService researchService) : ControllerBase
{
    /// <summary>
    /// Initiates a research workflow for the given topic.
    /// </summary>
    /// <param name="request">The research request containing topic, audience, and writing style</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// 200 OK - Research completed successfully with artifacts
    /// 400 Bad Request - Topic is required or invalid input
    /// 500 Internal Server Error - Research workflow execution failed
    /// </returns>
    [HttpPost("ConductResearch")]
    [ProducesResponseType(typeof(ResearchResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConductResearch([FromBody] ConductResearchRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest("Topic is required.");

        var researchArtifacts = await researchService.ConductResearchAsync(request.Topic, request.Audience, request.WritingStyle, cancellationToken).ConfigureAwait(false);
        return Ok(new ResearchResultsResponse { ResearchArtifacts = researchArtifacts });
    }
}
