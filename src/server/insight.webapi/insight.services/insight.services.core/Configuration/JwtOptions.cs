namespace Insight.Services.Core.Configuration;

public class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "https://insightforge.local";
    public string Audience { get; set; } = "insight-forge-api";
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
