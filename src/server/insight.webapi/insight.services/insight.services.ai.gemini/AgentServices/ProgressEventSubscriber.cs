using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.AgentServices;

/// <summary>
/// Subscribes to event bus streams and tracks progress metrics
/// </summary>
public class ProgressEventSubscriber
{
    private readonly IJobAgentService _jobAgentService;
    private readonly IProgressMetricsService _progressMetricsService;
    private readonly ILogger<ProgressEventSubscriber> _logger;

    public ProgressEventSubscriber(
        IJobAgentService jobAgentService,
        IProgressMetricsService progressMetricsService,
        ILogger<ProgressEventSubscriber> logger)
    {
        _jobAgentService = jobAgentService;
        _progressMetricsService = progressMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to events for a job and track progress
    /// </summary>
    public async Task SubscribeToProgressAsync(string jobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        var eventBus = _jobAgentService.GetEventBus(jobId);
        if (eventBus == null)
        {
            _logger.LogWarning("No event bus found for job {JobId}", jobId);
            return;
        }

        _logger.LogInformation("Started tracking progress for job {JobId}", jobId);

        try
        {
            await foreach (var @event in eventBus.SubscribeAsync(cancellationToken))
            {
                _progressMetricsService.TrackEvent(jobId, @event);

                if (@event.EventType == AgentEventType.InteractionComplete)
                {
                    _logger.LogInformation("Job {JobId} completed", jobId);
                    break;
                }

                if (@event.EventType == AgentEventType.Error)
                {
                    _logger.LogError("Job {JobId} error: {@Error}", jobId, @event.Error);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Progress tracking cancelled for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking progress for job {JobId}", jobId);
        }
    }
}
