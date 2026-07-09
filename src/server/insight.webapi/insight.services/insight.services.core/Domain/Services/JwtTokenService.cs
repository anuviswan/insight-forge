using Insight.Services.Core.Configuration;
using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Insight.Services.Core.Domain.Services;

/// <summary>
/// Implementation of JWT token generation and validation.
/// Uses IOptions pattern for configuration management.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwt;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _jwt = options.Value.Jwt;
        if (string.IsNullOrEmpty(_jwt.SecretKey))
            throw new InvalidOperationException("JWT secret key not configured");
    }

    /// <summary>
    /// Generate a JWT access token.
    /// Default expiry: 15 minutes (overridable).
    /// </summary>
    public string GenerateAccessToken(UserTokenClaims claims, TimeSpan? expiresIn = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, claims.UserId),
            new Claim(ClaimTypes.Email, claims.Email),
            new Claim("token_type", "access")
        };

        foreach (var role in claims.Roles)
        {
            tokenClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: tokenClaims,
            expires: DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromMinutes(_jwt.AccessTokenExpiryMinutes)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate a JWT refresh token.
    /// Default expiry: 7 days (overridable).
    /// </summary>
    public string GenerateRefreshToken(UserTokenClaims claims, TimeSpan? expiresIn = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, claims.UserId),
            new Claim("token_type", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: tokenClaims,
            expires: DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromDays(_jwt.RefreshTokenExpiryDays)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validate JWT token and extract claims.
    /// Returns null if token is invalid or expired.
    /// Never throws; returns null on validation failure.
    /// </summary>
    public UserTokenClaims? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = principal.FindFirst(ClaimTypes.Email);

            if (userIdClaim == null)
                return null;

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return new UserTokenClaims
            {
                UserId = userIdClaim.Value,
                Email = emailClaim?.Value ?? string.Empty,
                Roles = roles
            };
        }
        catch
        {
            return null;
        }
    }
}
