using Insight.Services.Interfaces.Ai;
using Insight.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

#nullable enable

namespace Insight.WebApi.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentStatusController : ControllerBase
{
    private readonly IJobAgentService _jobAgentService;
    private readonly ILogger<AgentStatusController> _logger;

    public AgentStatusController(IJobAgentService jobAgentService, ILogger<AgentStatusController> logger)
    {
        _jobAgentService = jobAgentService;
        _logger = logger;
    }

    /// <summary>
    /// Stream agent status events via Server-Sent Events (SSE)
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server-Sent Events stream</returns>
    /// <remarks>
    /// Connect to this endpoint to receive real-time agent status updates.
    /// The stream sends events in SSE format (data: {...}\n\n).
    ///
    /// Example client usage:
    /// ```javascript
    /// const eventSource = new EventSource(`/api/agent/blog/${jobId}/stream`);
    /// eventSource.onmessage = (event) => {
    ///   const statusEvent = JSON.parse(event.data);
    ///   console.log(statusEvent);
    /// };
    /// ```
    /// </remarks>
    [HttpGet("blog/{jobId}/stream")]
    [Produces("text/event-stream")]
    public async Task GetAgentStatus(string jobId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("jobId is required", cancellationToken);
            return;
        }

        _logger.LogInformation("Client connected to stream for job {JobId}", jobId);

        // Set SSE headers
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers["Cache-Control"] = "no-cache";
        HttpContext.Response.Headers["Connection"] = "keep-alive";

        try
        {
            var eventBus = _jobAgentService.GetEventBus(jobId);
            if (eventBus == null)
            {
                _logger.LogWarning("No active event bus for job {JobId}", jobId);
                await SendSseEvent(HttpContext.Response, new
                {
                    error = "Job not found or not active",
                    jobId = jobId
                }, cancellationToken);
                return;
            }

            // Subscribe to events and stream them
            await foreach (var @event in eventBus.SubscribeAsync(cancellationToken))
            {
                var dto = AgentStatusEventDto.FromDomain(@event);
                await SendSseEvent(HttpContext.Response, dto, cancellationToken);
            }

            _logger.LogInformation("Stream completed for job {JobId}", jobId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client disconnected from stream for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming events for job {JobId}", jobId);
            await SendSseEvent(HttpContext.Response, new
            {
                error = "Stream error: " + ex.Message,
                jobId = jobId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Get the current progress of a job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <returns>Current job status or 404 if job not found</returns>
    [HttpGet("blog/{jobId}/progress")]
    public IActionResult GetJobProgress(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required");

        if (!_jobAgentService.IsJobActive(jobId))
            return NotFound(new { message = $"Job {jobId} not found" });

        return Ok(new { jobId = jobId, isActive = true });
    }

    /// <summary>
    /// Get list of active jobs
    /// </summary>
    /// <returns>List of active job IDs</returns>
    [HttpGet("jobs")]
    public IActionResult GetActiveJobs()
    {
        var jobs = _jobAgentService.GetActiveJobs();
        return Ok(new { jobs = jobs.ToList(), count = jobs.Count() });
    }

    /// <summary>
    /// Send a Server-Sent Event to the response stream
    /// </summary>
    private static async Task SendSseEvent(HttpResponse response, object data, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(data);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
