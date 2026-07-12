namespace Insight.Services.Interfaces.Ai.Events;

public class ErrorData
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Retryable { get; set; }
}
