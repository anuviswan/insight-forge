using Insight.Services.Interfaces.Ai.Events;

namespace Insight.Services.Interfaces.Ai;

public interface IJobAgentService
{
    /// <summary>
    /// Get or create an event bus for a job
    /// </summary>
    IEventBus GetOrCreateEventBus(string jobId);

    /// <summary>
    /// Get an existing event bus for a job
    /// </summary>
    IEventBus? GetEventBus(string jobId);

    /// <summary>
    /// Complete a job and clean up resources
    /// </summary>
    void CompleteJob(string jobId);

    /// <summary>
    /// Get all active job IDs
    /// </summary>
    IEnumerable<string> GetActiveJobs();

    /// <summary>
    /// Check if a job is active
    /// </summary>
    bool IsJobActive(string jobId);
}
