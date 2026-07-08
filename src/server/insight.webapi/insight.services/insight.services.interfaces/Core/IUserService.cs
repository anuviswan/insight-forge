namespace Insight.Services.Interfaces.Core;

public interface IUserService
{
    /// <summary>
    /// Register a new user with email and password.
    /// Validates input, checks email uniqueness, hashes password, sends verification email.
    /// </summary>
    Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user email using verification token.
    /// Marks user as verified and clears verification token.
    /// </summary>
    Task<EmailVerificationResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email address.
    /// </summary>
    Task<object?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user exists by email.
    /// </summary>
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by ID (GUID).
    /// </summary>
    Task<object?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update last login timestamp for user.
    /// </summary>
    Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default);
}
