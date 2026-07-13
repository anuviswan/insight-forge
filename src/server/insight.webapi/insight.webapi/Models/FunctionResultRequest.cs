namespace Insight.WebApi.Models;

/// <summary>
/// Request to submit function execution result
/// </summary>
public class FunctionResultRequest
{
    /// <summary>
    /// Function call ID returned in the FunctionCalled event
    /// </summary>
    public string FunctionId { get; set; } = string.Empty;

    /// <summary>
    /// Execution result as JSON string or plain text
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Optional error message if execution failed
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Response for function result submission
/// </summary>
public class FunctionResultResponse
{
    /// <summary>
    /// Whether the result was accepted
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if submission failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Job ID being processed
    /// </summary>
    public string JobId { get; set; } = string.Empty;
}
