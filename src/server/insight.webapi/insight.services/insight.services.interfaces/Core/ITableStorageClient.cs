namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Abstraction for Azure Table Storage operations.
/// Simplifies data access and allows testing with mocks.
/// </summary>
public interface ITableStorageClient
{
    /// <summary>
    /// Get a user by ID (GUID string).
    /// </summary>
    Task<object?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a user by email address.
    /// Performs partition-key query (efficient within Users partition).
    /// </summary>
    Task<object?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user with given email exists.
    /// </summary>
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new user entity in table storage.
    /// </summary>
    Task CreateUserAsync(object user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing user entity.
    /// </summary>
    Task UpdateUserAsync(object user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new email verification token.
    /// </summary>
    Task CreateVerificationTokenAsync(object verification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a verification token by token value.
    /// </summary>
    Task<object?> GetVerificationTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a verification token (mark as used, etc).
    /// </summary>
    Task UpdateVerificationTokenAsync(object verification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new login attempt record.
    /// </summary>
    Task CreateLoginAttemptAsync(object attempt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get login attempts for an email address.
    /// </summary>
    Task<List<object>> GetLoginAttemptsAsync(string email, CancellationToken cancellationToken = default);
}
