namespace Insight.Services.Interfaces.Core;

public class EmailVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
