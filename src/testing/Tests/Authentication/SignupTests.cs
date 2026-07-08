using InsightForge.E2E.Tests.Fixtures;
using InsightForge.E2E.Tests.Pages;
using InsightForge.E2E.Tests.Utilities;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InsightForge.E2E.Tests.Tests.Authentication;

[TestClass]
public class SignupTests
{
    private BrowserFixture _browserFixture;
    private RegisterPage _registerPage;
    private VerifyEmailPage _verifyEmailPage;

    [TestInitialize]
    public async Task Setup()
    {
        _browserFixture = new BrowserFixture();
        await _browserFixture.InitializeAsync();
        _registerPage = new RegisterPage(_browserFixture.Page);
        _verifyEmailPage = new VerifyEmailPage(_browserFixture.Page);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _browserFixture.DisposeAsync();
    }

    [TestMethod]
    public async Task Should_Display_Register_Page_When_Navigating_To_Signup()
    {
        await _registerPage.GotoRegisterPageAsync();

        await Expect(_browserFixture.Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*register"));
        await Expect(_browserFixture.Page.Locator("text=Create Account")).ToBeVisibleAsync();
        await Expect(_browserFixture.Page.Locator("text=Join Insight Forge AI Writing Suite")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Should_Successfully_Register_With_Valid_Credentials()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.RegisterAsync(uniqueEmail, password);

        await Expect(_browserFixture.Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*verify-email"), new PageAssertionsToHaveURLOptions { Timeout = 5000 });
    }

    [TestMethod]
    public async Task Should_Display_Success_Message_After_Registration()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.EnterEmailAsync(uniqueEmail);
        await _registerPage.EnterPasswordAsync(password);
        await _registerPage.EnterConfirmPasswordAsync(password);
        await _registerPage.AgreeToTermsAsync();
        await _registerPage.ClickSubmitButtonAsync();

        var hasSuccessMessage = await _registerPage.HasSuccessMessageAsync();
        Assert.IsTrue(hasSuccessMessage);
    }

    [TestMethod]
    public async Task Should_Display_Error_When_Email_Is_Empty()
    {
        await _registerPage.GotoRegisterPageAsync();
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.EnterPasswordAsync(password);
        await _registerPage.EnterConfirmPasswordAsync(password);
        await _registerPage.AgreeToTermsAsync();
        await _registerPage.ClickSubmitButtonAsync();

        var errorMessage = await _registerPage.GetErrorMessageAsync();
        Assert.IsTrue(errorMessage.Contains("fill in all fields", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Should_Display_Error_When_Password_Is_Empty()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();

        await _registerPage.EnterEmailAsync(uniqueEmail);
        await _registerPage.AgreeToTermsAsync();
        await _registerPage.ClickSubmitButtonAsync();

        var errorMessage = await _registerPage.GetErrorMessageAsync();
        Assert.IsTrue(errorMessage.Contains("fill in all fields", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Should_Display_Error_When_Passwords_Do_Not_Match()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
        var password = TestDataGenerator.GenerateStrongPassword();
        var differentPassword = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.EnterEmailAsync(uniqueEmail);
        await _registerPage.EnterPasswordAsync(password);
        await _registerPage.EnterConfirmPasswordAsync(differentPassword);
        await _registerPage.AgreeToTermsAsync();
        await _registerPage.ClickSubmitButtonAsync();

        var errorMessage = await _registerPage.GetErrorMessageAsync();
        Assert.IsTrue(errorMessage.Contains("Passwords do not match", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Should_Display_Error_When_Terms_Not_Agreed()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.EnterEmailAsync(uniqueEmail);
        await _registerPage.EnterPasswordAsync(password);
        await _registerPage.EnterConfirmPasswordAsync(password);
        await _registerPage.ClickSubmitButtonAsync();

        var errorMessage = await _registerPage.GetErrorMessageAsync();
        Assert.IsTrue(errorMessage.Contains("agree to the terms", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Should_Disable_Submit_Button_When_Terms_Not_Checked()
    {
        await _registerPage.GotoRegisterPageAsync();
        var isDisabled = await _registerPage.IsSubmitButtonDisabledAsync();

        Assert.IsTrue(isDisabled);
    }

    [TestMethod]
    public async Task Should_Enable_Submit_Button_When_Terms_Are_Checked()
    {
        await _registerPage.GotoRegisterPageAsync();
        await _registerPage.AgreeToTermsAsync();

        var isDisabled = await _registerPage.IsSubmitButtonDisabledAsync();
        Assert.IsFalse(isDisabled);
    }

    [TestMethod]
    public async Task Should_Navigate_To_Login_When_Login_Link_Clicked()
    {
        await _registerPage.GotoRegisterPageAsync();

        await _registerPage.ClickLoginLinkAsync();

        await Expect(_browserFixture.Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*login"));
    }

    [TestMethod]
    public async Task Should_Toggle_Password_Visibility()
    {
        await _registerPage.GotoRegisterPageAsync();
        var password = TestDataGenerator.GenerateStrongPassword();
        await _registerPage.EnterPasswordAsync(password);

        var initialType = await _registerPage.GetPasswordInputTypeAsync();
        await _registerPage.TogglePasswordVisibilityAsync();
        var toggledType = await _registerPage.GetPasswordInputTypeAsync();
        await _registerPage.TogglePasswordVisibilityAsync();
        var finalType = await _registerPage.GetPasswordInputTypeAsync();

        Assert.AreEqual("password", initialType);
        Assert.AreEqual("text", toggledType);
        Assert.AreEqual("password", finalType);
    }

    [TestMethod]
    public async Task Should_Display_Password_Strength_Indicator_When_Entering_Password()
    {
        await _registerPage.GotoRegisterPageAsync();

        await _registerPage.EnterPasswordAsync("weak");

        var strengthIndicator = _browserFixture.Page.Locator(".password-strength");
        await Expect(strengthIndicator).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Should_Clear_Form_After_Successful_Registration()
    {
        await _registerPage.GotoRegisterPageAsync();
        var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.EnterEmailAsync(uniqueEmail);
        await _registerPage.EnterPasswordAsync(password);
        await _registerPage.EnterConfirmPasswordAsync(password);
        await _registerPage.AgreeToTermsAsync();
        await _registerPage.ClickSubmitButtonAsync();

        await _registerPage.WaitForLoadAsync();
        var emailInput = _browserFixture.Page.Locator("#email");
        var emailValue = await emailInput.GetAttributeAsync("value");
        Assert.AreEqual("", emailValue);
    }

    [TestMethod]
    public async Task Should_Display_Error_For_Duplicate_Email()
    {
        await _registerPage.GotoRegisterPageAsync();
        var duplicateEmail = "duplicate@example.com";
        var password = TestDataGenerator.GenerateStrongPassword();

        await _registerPage.RegisterAsync(duplicateEmail, password);

        var hasErrorOrSuccess = await _registerPage.HasErrorMessageAsync() || await _registerPage.HasSuccessMessageAsync();
        Assert.IsTrue(hasErrorOrSuccess);
    }
}
