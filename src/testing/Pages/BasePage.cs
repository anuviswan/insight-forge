using Microsoft.Playwright;

namespace InsightForge.E2E.Tests.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly TestConfiguration.TestConfiguration Configuration;

    protected BasePage(IPage page)
    {
        Page = page;
        Configuration = new TestConfiguration.TestConfiguration();
    }

    public async Task GotoAsync(string path)
    {
        var url = $"{Configuration.BaseUrl}{path}";
        await Page.GotoAsync(url);
    }

    public async Task WaitForLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected ILocator GetByLabel(string label)
    {
        return Page.GetByLabel(label);
    }

    protected ILocator GetByRole(AriaRole role, string? name = null)
    {
        return name == null
            ? Page.GetByRole(role)
            : Page.GetByRole(role, new PageGetByRoleOptions { Name = name });
    }

    protected ILocator GetByPlaceholder(string placeholder)
    {
        return Page.GetByPlaceholder(placeholder);
    }

    protected ILocator GetById(string id)
    {
        return Page.Locator($"#{id}");
    }

    protected ILocator GetByTestId(string testId)
    {
        return Page.GetByTestId(testId);
    }
}
