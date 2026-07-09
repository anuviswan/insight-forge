namespace Insight.Services.Core.Options;

/// <summary>
/// Configuration options for login attempt rate limiting.
/// </summary>
public class LoginAttemptOptions
{
    public const string SectionName = "LoginAttempts";

    /// <summary>
    /// Maximum number of failed login attempts before account lockout.
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Duration in minutes that an account remains locked after exceeding max failed attempts.
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;
}
