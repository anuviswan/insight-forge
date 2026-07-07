namespace Insight.WebApi.Models.Auth;

public class RegistrationResponse
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}
