namespace Insight.Services.Core.Domain.Entities;

/// <summary>
/// Represents an email verification token in Azure Table Storage.
/// Enables one-time-use verification links with expiry.
/// </summary>
public class EmailVerificationEntity : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
