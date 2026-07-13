using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.AgentServices;

/// <summary>
/// Manages function execution state and resumption for multi-turn workflows
/// </summary>
public class FunctionResultService : IFunctionResultService
{
    private readonly IJobAgentService _jobAgentService;
    private readonly ILogger<FunctionResultService> _logger;
    private readonly Dictionary<string, FunctionExecutionState> _executionStates = new();
    private readonly object _lock = new();

    public FunctionResultService(
        IJobAgentService jobAgentService,
        ILogger<FunctionResultService> logger)
    {
        _jobAgentService = jobAgentService;
        _logger = logger;
    }

    public async Task RegisterFunctionCallAsync(
        string jobId,
        FunctionCallDetails details,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));
        ArgumentNullException.ThrowIfNull(details, nameof(details));

        lock (_lock)
        {
            _executionStates[jobId] = new FunctionExecutionState
            {
                JobId = jobId,
                FunctionCall = details,
                RegisteredAt = DateTime.UtcNow
            };
        }

        _logger.LogInformation(
            "Registered function call {FunctionName} (ID: {FunctionId}) for job {JobId}",
            details.FunctionName, details.FunctionId, jobId);

        // Publish FunctionCalled event to pause streaming
        var eventBus = _jobAgentService.GetEventBus(jobId);
        if (eventBus != null)
        {
            var pauseEvent = new AgentStatusEvent
            {
                EventId = Guid.NewGuid(),
                InteractionId = details.InteractionId ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                EventType = AgentEventType.FunctionCalled,
                Status = $"Function call paused: {details.FunctionName}",
                Data = new Dictionary<string, object>
                {
                    { "function_name", details.FunctionName },
                    { "function_id", details.FunctionId },
                    { "arguments", details.Arguments ?? new Dictionary<string, object>() }
                }
            };

            await eventBus.PublishAsync(pauseEvent, cancellationToken);
        }
    }

    public FunctionCallDetails? GetPendingFunctionCall(string jobId)
    {
        lock (_lock)
        {
            if (_executionStates.TryGetValue(jobId, out var state) && !state.FunctionCall.Executed)
            {
                return state.FunctionCall;
            }
        }

        return null;
    }

    public async Task<bool> SubmitFunctionResultAsync(
        string jobId,
        string functionId,
        string result,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));
        ArgumentException.ThrowIfNullOrWhiteSpace(functionId, nameof(functionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(result, nameof(result));

        lock (_lock)
        {
            if (!_executionStates.TryGetValue(jobId, out var state))
            {
                _logger.LogWarning("No execution state found for job {JobId}", jobId);
                return false;
            }

            if (state.FunctionCall.FunctionId != functionId)
            {
                _logger.LogWarning(
                    "Function ID mismatch for job {JobId}. Expected {Expected}, got {Actual}",
                    jobId, state.FunctionCall.FunctionId, functionId);
                return false;
            }

            state.FunctionCall.Executed = true;
            state.ExecutionResult = new FunctionExecutionResult
            {
                FunctionId = functionId,
                Result = result,
                Success = true
            };
        }

        _logger.LogInformation(
            "Received function result for {FunctionName} (ID: {FunctionId}) on job {JobId}",
            GetPendingFunctionCall(jobId)?.FunctionName ?? "unknown",
            functionId, jobId);

        // Publish FunctionCompleted event to signal resumption
        var eventBus = _jobAgentService.GetEventBus(jobId);
        if (eventBus != null)
        {
            var resumeEvent = new AgentStatusEvent
            {
                EventId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = AgentEventType.FunctionCompleted,
                Status = "Function executed, resuming stream",
                Data = new Dictionary<string, object>
                {
                    { "function_id", functionId },
                    { "result", result }
                }
            };

            await eventBus.PublishAsync(resumeEvent, cancellationToken);
        }

        return true;
    }

    public bool HasPendingFunctionCall(string jobId)
    {
        lock (_lock)
        {
            return _executionStates.TryGetValue(jobId, out var state) &&
                   !state.FunctionCall.Executed;
        }
    }

    public void ClearFunctionState(string jobId)
    {
        lock (_lock)
        {
            if (_executionStates.Remove(jobId))
            {
                _logger.LogInformation("Cleared function execution state for job {JobId}", jobId);
            }
        }
    }

    private class FunctionExecutionState
    {
        public string JobId { get; set; } = string.Empty;
        public FunctionCallDetails FunctionCall { get; set; } = null!;
        public FunctionExecutionResult? ExecutionResult { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
