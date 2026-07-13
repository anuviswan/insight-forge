namespace Insight.Services.Interfaces.Ai;

/// <summary>
/// Manages function execution state and resumption for multi-turn workflows
/// </summary>
public interface IFunctionResultService
{
    /// <summary>
    /// Register a function call that needs execution (pause streaming)
    /// </summary>
    Task RegisterFunctionCallAsync(string jobId, FunctionCallDetails details, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending function call for a job
    /// </summary>
    FunctionCallDetails? GetPendingFunctionCall(string jobId);

    /// <summary>
    /// Submit execution result for a function call and resume streaming
    /// </summary>
    Task<bool> SubmitFunctionResultAsync(string jobId, string functionId, string result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if job has pending function execution
    /// </summary>
    bool HasPendingFunctionCall(string jobId);

    /// <summary>
    /// Clear function execution state for a job (cleanup)
    /// </summary>
    void ClearFunctionState(string jobId);
}

/// <summary>
/// Details of a function call that requires execution
/// </summary>
public class FunctionCallDetails
{
    public string FunctionId { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
    public string? InteractionId { get; set; }
    public int StepIndex { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Executed { get; set; }
}

/// <summary>
/// Result of function execution to resume streaming
/// </summary>
public class FunctionExecutionResult
{
    public string FunctionId { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}
