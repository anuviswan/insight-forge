using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace InsightForge.E2E.Tests.Pages;

public class VerifyEmailPage : BasePage
{
    public VerifyEmailPage(IPage page) : base(page)
    {
    }

    public async Task<bool> IsPageVisibleAsync()
    {
        try
        {
            await Expect(Page).ToHaveURLAsync("**/verify-email");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsVerificationMessageVisibleAsync()
    {
        try
        {
            var verificationMessage = Page.Locator("text=/verify|email|check/i").First;
            await Expect(verificationMessage).ToBeVisibleAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
