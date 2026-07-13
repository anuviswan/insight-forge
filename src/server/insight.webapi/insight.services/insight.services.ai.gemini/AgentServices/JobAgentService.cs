using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class JobAgentService : IJobAgentService
{
    private readonly Dictionary<string, IEventBus> _activeJobs = new();
    private readonly ILogger<JobAgentService> _logger;
    private readonly object _lock = new();

    public JobAgentService(ILogger<JobAgentService> logger)
    {
        _logger = logger;
    }

    public IEventBus GetOrCreateEventBus(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            if (_activeJobs.TryGetValue(jobId, out var eventBus))
            {
                return eventBus;
            }

            var newEventBus = new EventBusChannels(capacity: 100);
            _activeJobs[jobId] = newEventBus;
            _logger.LogInformation("Created event bus for job {JobId}", jobId);
            return newEventBus;
        }
    }

    public IEventBus? GetEventBus(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            _activeJobs.TryGetValue(jobId, out var eventBus);
            return eventBus;
        }
    }

    public void CompleteJob(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            if (_activeJobs.Remove(jobId, out var eventBus))
            {
                if (eventBus is EventBusChannels channels)
                {
                    channels.Complete();
                }
                _logger.LogInformation("Completed event bus for job {JobId}", jobId);
            }
        }
    }

    public IEnumerable<string> GetActiveJobs()
    {
        lock (_lock)
        {
            return _activeJobs.Keys.ToList();
        }
    }

    public bool IsJobActive(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            return _activeJobs.ContainsKey(jobId);
        }
    }
}
