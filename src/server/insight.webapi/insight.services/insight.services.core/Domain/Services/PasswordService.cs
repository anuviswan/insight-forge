using Insight.Services.Interfaces.Core;
using System.Text.RegularExpressions;

namespace Insight.Services.Core.Domain.Services;

/// <summary>
/// Implementation of password hashing and validation.
/// Uses BCrypt for secure password storage.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int BcryptWorkFactor = 12; // Cost factor for BCrypt hashing
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Hash a plaintext password using BCrypt.
    /// Never logs the plaintext password.
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);
    }

    /// <summary>
    /// Verify plaintext password against BCrypt hash.
    /// Uses BCrypt's built-in timing-safe comparison.
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // BCrypt throws on invalid hash format; treat as verification failure
            return false;
        }
    }

    /// <summary>
    /// Validate password strength.
    /// Requirements:
    /// - Minimum 8 characters
    /// - At least one letter (upper or lower)
    /// - At least one number
    /// </summary>
    public PasswordValidationResult ValidatePasswordStrength(string password)
    {
        var result = new PasswordValidationResult { IsValid = true };

        if (string.IsNullOrEmpty(password))
        {
            result.IsValid = false;
            result.Errors.Add("Password is required");
            return result;
        }

        if (password.Length < MinPasswordLength)
        {
            result.IsValid = false;
            result.Errors.Add($"Password must be at least {MinPasswordLength} characters long");
        }

        if (!Regex.IsMatch(password, @"[a-zA-Z]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one letter");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one number");
        }

        return result;
    }
}
