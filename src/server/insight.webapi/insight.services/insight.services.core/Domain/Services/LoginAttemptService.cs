using Insight.Services.Core.Domain.Entities;
using Insight.Services.Core.Options;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Insight.Services.Core.Domain.Services;

/// <summary>
/// Service for managing login attempts and enforcing rate limiting/account lockout.
/// </summary>
public class LoginAttemptService(
    ITableStorageClient storage,
    ILogger<LoginAttemptService> logger,
    IOptions<LoginAttemptOptions> options) : ILoginAttemptService
{
    private readonly ITableStorageClient _storage = storage;
    private readonly ILogger<LoginAttemptService> _logger = logger;
    private readonly LoginAttemptOptions _options = options.Value;

    /// <summary>
    /// Record a login attempt (successful or failed).
    /// Updates user's failed attempt counter if unsuccessful.
    /// </summary>
    public async Task RecordAttemptAsync(string email, string? userId, bool isSuccessful, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create login attempt record
            var attempt = new LoginAttemptEntity
            {
                RowKey = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}",
                Email = email,
                UserId = userId,
                IsSuccessful = isSuccessful,
                AttemptedAt = DateTime.UtcNow
            };

            await _storage.CreateLoginAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);

            if (isSuccessful)
            {
                _logger.LogInformation("Successful login recorded for user {UserId} ({Email})", userId, email);
            }
            else
            {
                _logger.LogWarning("Failed login attempt recorded for email {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording login attempt for email {Email}", email);
        }
    }

    /// <summary>
    /// Check if account is currently locked due to failed attempts.
    /// </summary>
    public async Task<bool> IsAccountLockedAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _storage.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false) as UserEntity;
            if (user == null)
                return false;

            if (!user.IsLockedOut)
                return false;

            // Check if lockout has expired
            if (user.LockedOutUntil == null || user.LockedOutUntil <= DateTime.UtcNow)
            {
                // Lockout expired, clear it
                user.IsLockedOut = false;
                user.LockedOutUntil = null;
                user.FailedLoginAttempts = 0;
                await _storage.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking lockout status for email {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Get time remaining until lockout expires (null if not locked).
    /// </summary>
    public async Task<TimeSpan?> GetLockoutTimeRemainingAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _storage.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false) as UserEntity;
            if (user == null || !user.IsLockedOut || user.LockedOutUntil == null)
                return null;

            var timeRemaining = user.LockedOutUntil.Value - DateTime.UtcNow;
            return timeRemaining > TimeSpan.Zero ? timeRemaining : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lockout time for email {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Increment failed login attempts and lock account if threshold exceeded.
    /// </summary>
    public async Task IncrementFailedAttemptsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _storage.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false) as UserEntity;
            if (user == null)
                return;

            user.FailedLoginAttempts++;
            user.LastFailedLoginAt = DateTime.UtcNow;

            if (user.FailedLoginAttempts >= _options.MaxFailedAttempts)
            {
                user.IsLockedOut = true;
                user.LockedOutUntil = DateTime.UtcNow.AddMinutes(_options.LockoutDurationMinutes);
                _logger.LogWarning("Account locked for email {Email} due to {Attempts} failed attempts", email, user.FailedLoginAttempts);
            }

            await _storage.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing failed attempts for email {Email}", email);
        }
    }
}
