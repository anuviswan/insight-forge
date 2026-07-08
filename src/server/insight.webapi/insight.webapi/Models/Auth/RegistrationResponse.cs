namespace Insight.WebApi.Models.Auth;

public record RegistrationResponse(
    bool Success,
    string? UserId,
    string Message,
    string? ErrorCode,
    List<string> ValidationErrors,
    string? VerificationToken = null)
{
    public RegistrationResponse() : this(false, null, string.Empty, null, new(), null) { }
}
