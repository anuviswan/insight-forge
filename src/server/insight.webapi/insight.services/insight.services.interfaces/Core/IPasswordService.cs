namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Service interface for password hashing and validation.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash a plaintext password using BCrypt.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify a plaintext password against a BCrypt hash.
    /// Uses timing-safe comparison to prevent timing attacks.
    /// </summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Validate password strength.
    /// Requirements: minimum 8 characters, at least one letter, at least one number.
    /// </summary>
    PasswordValidationResult ValidatePasswordStrength(string password);
}
