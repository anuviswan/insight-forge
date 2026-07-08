namespace Insight.Services.Interfaces.Core;

public class UserRegistrationResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public string? VerificationToken { get; set; }
}
