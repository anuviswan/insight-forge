using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace InsightForge.E2E.Tests.Pages;

public class RegisterPage : BasePage
{
    private ILocator EmailInput => GetById("email");
    private ILocator PasswordInput => GetById("password");
    private ILocator ConfirmPasswordInput => GetById("confirm-password");
    private ILocator AgreeToTermsCheckbox => GetById("agree-terms");
    private ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Create Account" });
    private ILocator ErrorAlert => Page.Locator(".error-alert");
    private ILocator SuccessAlert => Page.Locator(".success-alert");
    private ILocator ValidationErrors => Page.Locator(".validation-error-item");
    private ILocator LoginLink => Page.Locator(".login-link");
    private ILocator PasswordToggleButton => Page.Locator(".password-toggle-btn").First;
    private ILocator ConfirmPasswordToggleButton => Page.Locator(".password-toggle-btn").Nth(1);

    public RegisterPage(IPage page) : base(page)
    {
    }

    public async Task GotoRegisterPageAsync()
    {
        await GotoAsync("/register");
        await WaitForLoadAsync();
    }

    public async Task EnterEmailAsync(string email)
    {
        await EmailInput.FillAsync(email);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await PasswordInput.FillAsync(password);
    }

    public async Task EnterConfirmPasswordAsync(string password)
    {
        await ConfirmPasswordInput.FillAsync(password);
    }

    public async Task AgreeToTermsAsync()
    {
        await AgreeToTermsCheckbox.CheckAsync();
    }

    public async Task ClickSubmitButtonAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task RegisterAsync(string email, string password)
    {
        await EnterEmailAsync(email);
        await EnterPasswordAsync(password);
        await EnterConfirmPasswordAsync(password);
        await AgreeToTermsAsync();
        await ClickSubmitButtonAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        await Expect(ErrorAlert).ToBeVisibleAsync();
        return await ErrorAlert.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetSuccessMessageAsync()
    {
        await Expect(SuccessAlert).ToBeVisibleAsync();
        return await SuccessAlert.TextContentAsync() ?? string.Empty;
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        return await ErrorAlert.IsVisibleAsync();
    }

    public async Task<bool> HasSuccessMessageAsync()
    {
        return await SuccessAlert.IsVisibleAsync();
    }

    public async Task<bool> IsSubmitButtonDisabledAsync()
    {
        return await SubmitButton.IsDisabledAsync();
    }

    public async Task<List<string>> GetValidationErrorsAsync()
    {
        var errors = new List<string>();
        var count = await ValidationErrors.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var errorText = await ValidationErrors.Nth(i).TextContentAsync();
            if (!string.IsNullOrEmpty(errorText))
            {
                errors.Add(errorText);
            }
        }

        return errors;
    }

    public async Task ClickLoginLinkAsync()
    {
        await LoginLink.ClickAsync();
    }

    public async Task WaitForNavigationToVerifyEmailAsync()
    {
        await Page.WaitForURLAsync("**/verify-email");
    }

    public async Task TogglePasswordVisibilityAsync()
    {
        await PasswordToggleButton.ClickAsync();
    }

    public async Task ToggleConfirmPasswordVisibilityAsync()
    {
        await ConfirmPasswordToggleButton.ClickAsync();
    }

    public async Task<string> GetPasswordInputTypeAsync()
    {
        return await PasswordInput.GetAttributeAsync("type") ?? string.Empty;
    }

    public async Task<bool> IsPasswordFieldVisibleAsync()
    {
        var type = await GetPasswordInputTypeAsync();
        return type == "text";
    }
}
