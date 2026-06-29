using Insight.Services.Interfaces.Ai;
using Insight.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Insight.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController(IAgent agentService) : ControllerBase
{
    [HttpPost("CreateAgent")]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.AgentName))
            return BadRequest("Agent name is required.");

        try
        {
            var agentId = await agentService.CreateAgent(request.AgentName, cancellationToken);
            return Ok(new CreateAgentResponse
            {
                AgentId = agentId,
                Status = "Created",
                Message = $"Agent '{request.AgentName}' created successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new CreateAgentResponse
            {
                Status = "NotFound",
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new CreateAgentResponse
            {
                Status = "BadRequest",
                Message = ex.Message
            });
        }
    }
}
