namespace Insight.WebApi.Models.Auth;

/// <summary>
/// Request model for user sign-in.
/// </summary>
public class SignInRequest
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
