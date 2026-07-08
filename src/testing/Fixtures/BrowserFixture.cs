using Microsoft.Playwright;

namespace InsightForge.E2E.Tests.Fixtures;

public class BrowserFixture : IAsyncLifetime
{
    private readonly TestConfiguration.TestConfiguration _configuration;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public IPage Page => _page ?? throw new InvalidOperationException("Page is not initialized");
    public IBrowserContext Context => _context ?? throw new InvalidOperationException("Context is not initialized");

    public BrowserFixture()
    {
        _configuration = new TestConfiguration.TestConfiguration();
    }

    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserLaunchOptions
        {
            Headless = _configuration.HeadlessMode,
            SlowMo = _configuration.SlowMo
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        _page = await _context.NewPageAsync();

        // Set timeout for all navigation and action operations
        _page.SetDefaultTimeout(_configuration.Timeout);
        _page.SetDefaultNavigationTimeout(_configuration.NavigationTimeout);
    }

    public async Task DisposeAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
        }

        if (_context != null)
        {
            await _context.CloseAsync();
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
    }
}
