namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Request model for user sign-in operation.
/// </summary>
public class UserSignInRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional flag to persist session across browser sessions.
    /// </summary>
    public bool RememberMe { get; set; }
}
