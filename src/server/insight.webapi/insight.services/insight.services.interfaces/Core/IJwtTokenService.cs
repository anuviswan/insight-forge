namespace Insight.Services.Interfaces.Core;

/// <summary>
/// JWT token claims information
/// </summary>
public class UserTokenClaims
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Service interface for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate a JWT access token for a user.
    /// </summary>
    /// <param name="claims">User claims to include in token</param>
    /// <param name="expiresIn">Token expiry duration</param>
    string GenerateAccessToken(UserTokenClaims claims, TimeSpan? expiresIn = null);

    /// <summary>
    /// Generate a JWT refresh token for a user.
    /// Refresh tokens have longer expiry (typically 7 days).
    /// </summary>
    string GenerateRefreshToken(UserTokenClaims claims, TimeSpan? expiresIn = null);

    /// <summary>
    /// Validate and extract claims from a JWT token.
    /// Returns null if token is invalid or expired.
    /// </summary>
    UserTokenClaims? ValidateToken(string token);
}
