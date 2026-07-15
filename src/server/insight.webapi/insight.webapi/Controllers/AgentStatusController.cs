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
    private readonly IStreamingResilienceMetrics _resilienceMetrics;
    private readonly ILogger<AgentStatusController> _logger;

    public AgentStatusController(
        IJobAgentService jobAgentService,
        IStreamingResilienceMetrics resilienceMetrics,
        ILogger<AgentStatusController> logger)
    {
        _jobAgentService = jobAgentService;
        _resilienceMetrics = resilienceMetrics;
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

        var eventBus = _jobAgentService.GetEventBus(jobId);
        if (eventBus == null)
        {
            _logger.LogWarning("No active event bus for job {JobId}", jobId);

            // 204 tells EventSource to fail the connection permanently rather than
            // auto-retry - the job either finished already or never existed, so
            // retrying against this URL will never succeed.
            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        // Set SSE headers
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers["Cache-Control"] = "no-cache";
        HttpContext.Response.Headers["Connection"] = "keep-alive";

        try
        {
            // Subscribe to events and stream them straight through to the client.
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
    /// Get streaming resilience metrics: error rates by type, retry success rate,
    /// and current event queue depth for each active job.
    /// </summary>
    /// <returns>Resilience metrics snapshot</returns>
    [HttpGet("metrics")]
    public IActionResult GetResilienceMetrics()
    {
        var snapshot = _resilienceMetrics.GetSnapshot();
        return Ok(snapshot);
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
