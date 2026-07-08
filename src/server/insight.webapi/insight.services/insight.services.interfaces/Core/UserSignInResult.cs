namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Result model for user sign-in operation.
/// </summary>
public class UserSignInResult
{
    /// <summary>
    /// Indicates whether the sign-in was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User ID if sign-in was successful.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// JWT access token if sign-in was successful.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// JWT refresh token if sign-in was successful.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Result message (generic on failure for security).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code for client-side handling (e.g., INVALID_CREDENTIALS, ACCOUNT_LOCKED).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Validation errors if any.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];
}
