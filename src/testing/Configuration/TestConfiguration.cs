using Microsoft.Extensions.Configuration;

namespace InsightForge.E2E.Tests.Configuration;

public class TestConfiguration
{
    private readonly IConfiguration _configuration;

    public TestConfiguration()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        _configuration = configBuilder.Build();
    }

    public string BaseUrl => _configuration["Playwright:BaseUrl"] ?? "http://localhost:5173";
    public bool HeadlessMode => bool.TryParse(_configuration["Playwright:HeadlessMode"], out var headless) && headless;
    public int Timeout => int.TryParse(_configuration["Playwright:Timeout"], out var timeout) ? timeout : 30000;
    public int NavigationTimeout => int.TryParse(_configuration["Playwright:NavigationTimeout"], out var navTimeout) ? navTimeout : 30000;
    public bool ScreenshotOnFailure => bool.TryParse(_configuration["Playwright:ScreenshotOnFailure"], out var screenshot) && screenshot;
    public int SlowMo => int.TryParse(_configuration["Playwright:SlowMo"], out var slowMo) ? slowMo : 0;

    public string ValidEmail => _configuration["TestData:ValidEmail"] ?? "testuser@example.com";
    public string ValidPassword => _configuration["TestData:ValidPassword"] ?? "SecurePassword123!";
}
