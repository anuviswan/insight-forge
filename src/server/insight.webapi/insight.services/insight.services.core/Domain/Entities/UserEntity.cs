namespace Insight.Services.Core.Domain.Entities;

/// <summary>
/// Represents a user entity stored in Azure Table Storage.
/// </summary>
public class UserEntity : BaseEntity
{
    public UserEntity()
    {
        PartitionKey = "Users";
    }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastFailedLoginAt { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockedOutUntil { get; set; }
}
