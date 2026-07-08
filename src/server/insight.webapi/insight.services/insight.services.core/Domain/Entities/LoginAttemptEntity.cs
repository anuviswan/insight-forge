namespace Insight.Services.Core.Domain.Entities;

/// <summary>
/// Represents a login attempt record stored in Azure Table Storage for audit trail.
/// </summary>
public class LoginAttemptEntity : BaseEntity
{
    public LoginAttemptEntity()
    {
        PartitionKey = "LoginAttempts";
    }

    /// <summary>
    /// Email address of the user attempting to log in.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User ID if login was successful, null otherwise.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Indicates whether the login attempt was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Timestamp of the login attempt.
    /// </summary>
    public DateTime AttemptedAt { get; set; }

    /// <summary>
    /// Timestamp when the record was created in storage.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
