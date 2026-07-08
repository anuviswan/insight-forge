namespace InsightForge.E2E.Tests.Utilities;

public static class TestDataGenerator
{
    public static string GenerateUniqueEmail()
    {
        var timestamp = DateTime.UtcNow.Ticks;
        var guid = Guid.NewGuid().ToString("N")[..8];
        return $"testuser_{guid}_{timestamp}@example.com";
    }

    public static string GenerateStrongPassword()
    {
        return $"SecurePassword{Guid.NewGuid().ToString("N")[..4]}!";
    }

    public static string GenerateWeakPassword()
    {
        return "weak";
    }

    public static string GenerateMediumPassword()
    {
        return "Password123";
    }
}
