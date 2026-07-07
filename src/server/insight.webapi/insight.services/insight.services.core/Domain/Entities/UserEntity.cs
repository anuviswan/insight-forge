using Azure;
using Azure.Data.Tables;

namespace Insight.Services.Core.Domain.Entities;

/// <summary>
/// Represents a user entity stored in Azure Table Storage.
/// </summary>
public class UserEntity : ITableEntity
{
    /// <summary>
    /// Partition key for all users (allows efficient scanning within partition).
    /// </summary>
    public string PartitionKey { get; set; } = "Users";

    /// <summary>
    /// Row key: unique user ID (GUID as string).
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (unique across system).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt-hashed password (never stored plaintext).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has verified their email address.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Email verification token (cleared after verification).
    /// </summary>
    public string? VerificationToken { get; set; }

    /// <summary>
    /// Expiry time for verification token (24 hours from creation).
    /// </summary>
    public DateTime? VerificationTokenExpiry { get; set; }

    /// <summary>
    /// Whether the user account is active (not deactivated).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp when user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of last successful login (null if never logged in).
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control.
    /// </summary>
    public ETag ETag { get; set; }

    /// <summary>
    /// Timestamp managed by Azure Tables.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
}
