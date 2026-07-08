using Insight.Services.Core.Domain.Entities;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Core.Domain.Services;

/// <summary>
/// User registration and management service.
/// Orchestrates password hashing, email validation, token generation.
/// </summary>
public class UserService : IUserService
{
    private readonly ITableStorageClient _storage;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    private readonly IHostEnvironment _environment;
    private const int VerificationTokenExpiryHours = 24;

    public UserService(
        ITableStorageClient storage,
        IPasswordService passwordService,
        IEmailService emailService,
        ILogger<UserService> logger,
        IHostEnvironment environment)
    {
        _storage = storage;
        _passwordService = passwordService;
        _emailService = emailService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Register a new user.
    /// Validates email, password strength, checks uniqueness, hashes password, sends verification email.
    /// </summary>
    public async Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = ValidateRegistrationRequest(request);
        if (!validationResult.IsValid)
        {
            return new UserRegistrationResult
            {
                Success = false,
                Message = "Registration validation failed",
                ErrorCode = "VALIDATION_ERROR",
                ValidationErrors = validationResult.Errors
            };
        }

        try
        {
            // Check email uniqueness
            var emailExists = await _storage.UserExistsAsync(request.Email, cancellationToken).ConfigureAwait(false);
            if (emailExists)
            {
                _logger.LogWarning("Registration attempted with duplicate email: {Email}", request.Email);
                return new UserRegistrationResult
                {
                    Success = false,
                    Message = "Email already registered",
                    ErrorCode = "DUPLICATE_EMAIL"
                };
            }

            // Validate password strength
            var passwordValidation = _passwordService.ValidatePasswordStrength(request.Password);
            if (!passwordValidation.IsValid)
            {
                return new UserRegistrationResult
                {
                    Success = false,
                    Message = "Password does not meet requirements",
                    ErrorCode = "WEAK_PASSWORD",
                    ValidationErrors = passwordValidation.Errors
                };
            }

            // Create user entity
            var userId = Guid.NewGuid().ToString();
            var passwordHash = _passwordService.HashPassword(request.Password);
            var verificationToken = GenerateVerificationToken();

            var user = new UserEntity
            {
                RowKey = userId,
                Email = request.Email,
                PasswordHash = passwordHash,
                IsVerified = false,
                VerificationToken = verificationToken,
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(VerificationTokenExpiryHours),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Save to table storage
            await _storage.CreateUserAsync(user, cancellationToken).ConfigureAwait(false);

            // Create verification token record
            var verification = new EmailVerificationEntity
            {
                PartitionKey = request.Email,
                RowKey = verificationToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddHours(VerificationTokenExpiryHours),
                IsUsed = false
            };

            await _storage.CreateVerificationTokenAsync(verification, cancellationToken).ConfigureAwait(false);

            // Send verification email (fire and forget - don't block response)
            _ = Task.Run(() => SendVerificationEmailAsync(request.Email, verificationToken, cancellationToken), cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId} ({Email})", userId, request.Email);

            // In development, include the verification token for testing
            var result = new UserRegistrationResult
            {
                Success = true,
                UserId = userId,
                Message = "Registration successful. Please check your email to verify your account.",
                VerificationToken = _environment.IsDevelopment() ? verificationToken : null
            };

            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Development mode: Verification token for {Email}: {Token}", request.Email, verificationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            return new UserRegistrationResult
            {
                Success = false,
                Message = "Registration failed due to server error",
                ErrorCode = "SERVER_ERROR"
            };
        }
    }

    /// <summary>
    /// Verify user email using verification token.
    /// Marks user as verified and prevents token reuse.
    /// </summary>
    public async Task<EmailVerificationResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new EmailVerificationResult
            {
                Success = false,
                Message = "Verification token is required",
                ErrorCode = "INVALID_TOKEN"
            };
        }

        try
        {
            // Get verification token record
            var verification = await _storage.GetVerificationTokenAsync(token, cancellationToken).ConfigureAwait(false) as EmailVerificationEntity;
            
            if (verification == null)
            {
                _logger.LogWarning("Verification attempted with invalid token");
                return new EmailVerificationResult
                {
                    Success = false,
                    Message = "Invalid verification token",
                    ErrorCode = "INVALID_TOKEN"
                };
            }

            // Check if token is expired
            if (verification.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Verification attempted with expired token for user {UserId}", verification.UserId);
                return new EmailVerificationResult
                {
                    Success = false,
                    Message = "Verification token has expired",
                    ErrorCode = "TOKEN_EXPIRED"
                };
            }

            // Check if token already used
            if (verification.IsUsed)
            {
                _logger.LogWarning("Verification attempted with already-used token for user {UserId}", verification.UserId);
                return new EmailVerificationResult
                {
                    Success = false,
                    Message = "Verification token has already been used",
                    ErrorCode = "TOKEN_ALREADY_USED"
                };
            }

            // Mark token as used
            verification.IsUsed = true;
            await _storage.UpdateVerificationTokenAsync(verification, cancellationToken).ConfigureAwait(false);

            // Get and update user
            var user = await _storage.GetUserByIdAsync(verification.UserId, cancellationToken).ConfigureAwait(false) as UserEntity;
            if (user == null)
            {
                _logger.LogError("User not found during email verification: {UserId}", verification.UserId);
                return new EmailVerificationResult
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                };
            }

            // Mark user as verified and clear token
            user.IsVerified = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            await _storage.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Email verified successfully for user {UserId}", verification.UserId);

            return new EmailVerificationResult
            {
                Success = true,
                Message = "Email verified successfully. You can now log in."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return new EmailVerificationResult
            {
                Success = false,
                Message = "Verification failed due to server error",
                ErrorCode = "SERVER_ERROR"
            };
        }
    }

    public async Task<object?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _storage.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _storage.UserExistsAsync(email, cancellationToken).ConfigureAwait(false);
    }

    public async Task<object?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _storage.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _storage.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false) as UserEntity;
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _storage.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        }
    }

    private RegistrationValidationResult ValidateRegistrationRequest(UserRegistrationRequest request)
    {
        var result = new RegistrationValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            result.IsValid = false;
            result.Errors.Add("Email is required");
        }
        else if (!IsValidEmail(request.Email))
        {
            result.IsValid = false;
            result.Errors.Add("Email format is invalid");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            result.IsValid = false;
            result.Errors.Add("Password is required");
        }

        if (request.Password != request.ConfirmPassword)
        {
            result.IsValid = false;
            result.Errors.Add("Passwords do not match");
        }

        return result;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    private async Task SendVerificationEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        try
        {
            var verificationLink = "https://localhost:5173/verify-email?token=" + Uri.EscapeDataString(token);
            await _emailService.SendVerificationEmailAsync(email, verificationLink, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
        }
    }
}

internal class RegistrationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
