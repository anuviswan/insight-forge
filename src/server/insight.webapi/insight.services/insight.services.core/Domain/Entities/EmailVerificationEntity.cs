using Azure;
using Azure.Data.Tables;

namespace Insight.Services.Core.Domain.Entities;

/// <summary>
/// Represents an email verification token in Azure Table Storage.
/// Enables one-time-use verification links with expiry.
/// </summary>
public class EmailVerificationEntity : ITableEntity
{
    /// <summary>
    /// Partition key: email address (enables lookup by email).
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Row key: verification token (base64 encoded random bytes).
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    /// <summary>
    /// User ID (GUID as string) associated with this verification.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when this verification token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this token has been used for verification.
    /// Prevents token reuse.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// UTC timestamp when this verification token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control.
    /// </summary>
    public ETag ETag { get; set; }

    /// <summary>
    /// Timestamp managed by Azure Tables.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
}
