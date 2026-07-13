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
    private readonly IProgressMetricsService _progressMetricsService;
    private readonly IFunctionResultService _functionResultService;
    private readonly ILogger<AgentStatusController> _logger;

    public AgentStatusController(
        IJobAgentService jobAgentService,
        IProgressMetricsService progressMetricsService,
        IFunctionResultService functionResultService,
        ILogger<AgentStatusController> logger)
    {
        _jobAgentService = jobAgentService;
        _progressMetricsService = progressMetricsService;
        _functionResultService = functionResultService;
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
    /// <returns>Detailed progress metrics or 404 if job not found</returns>
    [HttpGet("blog/{jobId}/progress")]
    public IActionResult GetJobProgress(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required");

        var metrics = _progressMetricsService.GetProgress(jobId);
        if (metrics == null)
            return NotFound(new { message = $"No progress data for job {jobId}" });

        var dto = JobProgressDto.FromDomain(metrics);
        return Ok(dto);
    }

    /// <summary>
    /// Get detailed progress including step history and event log
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <returns>Detailed progress with step history or 404 if job not found</returns>
    [HttpGet("blog/{jobId}/progress/detailed")]
    public IActionResult GetDetailedJobProgress(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required");

        var progress = _progressMetricsService.GetDetailedProgress(jobId);
        if (progress == null)
            return NotFound(new { message = $"No progress data for job {jobId}" });

        var dto = DetailedJobProgressDto.FromDomain(progress);
        return Ok(dto);
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
    /// Submit function execution result to resume streaming
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <param name="request">Function result with ID and execution output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Submission status</returns>
    [HttpPost("blog/{jobId}/function-result")]
    public async Task<IActionResult> SubmitFunctionResult(
        string jobId,
        [FromBody] FunctionResultRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest(new { error = "jobId is required" });

        if (request == null || string.IsNullOrWhiteSpace(request.FunctionId))
            return BadRequest(new { error = "FunctionId is required" });

        if (string.IsNullOrWhiteSpace(request.Result) && string.IsNullOrWhiteSpace(request.Error))
            return BadRequest(new { error = "Either Result or Error must be provided" });

        _logger.LogInformation(
            "Received function result for job {JobId}, functionId {FunctionId}",
            jobId, request.FunctionId);

        var result = await _functionResultService.SubmitFunctionResultAsync(
            jobId,
            request.FunctionId,
            request.Result ?? request.Error ?? string.Empty,
            cancellationToken);

        if (!result)
        {
            return BadRequest(new FunctionResultResponse
            {
                Success = false,
                Message = "Function result could not be processed. Job or function ID may not match.",
                JobId = jobId
            });
        }

        return Ok(new FunctionResultResponse
        {
            Success = true,
            Message = "Function result accepted, streaming resumed",
            JobId = jobId
        });
    }

    /// <summary>
    /// Get pending function call for a job (check if stream is paused)
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <returns>Pending function call details or 404 if none pending</returns>
    [HttpGet("blog/{jobId}/function-call/pending")]
    public IActionResult GetPendingFunctionCall(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required");

        var pending = _functionResultService.GetPendingFunctionCall(jobId);
        if (pending == null)
            return NotFound(new { message = "No pending function call for this job" });

        return Ok(new
        {
            functionId = pending.FunctionId,
            functionName = pending.FunctionName,
            arguments = pending.Arguments,
            timestamp = pending.Timestamp
        });
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
