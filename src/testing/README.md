# Insight Forge E2E Tests

End-to-end test automation for the Insight Forge application using Playwright and xUnit.

## Technology Stack

- **Test Framework**: xUnit
- **UI Automation**: Microsoft Playwright
- **Language**: C#
- **Pattern**: Page Object Model (POM)

## Project Structure

```
Tests/
├── Authentication/          # Authentication-related tests
│   └── SignupTests.cs      # User registration tests
├── Pages/                   # Page Object Model classes
│   ├── BasePage.cs         # Base page with common locators
│   ├── RegisterPage.cs     # Registration page
│   └── VerifyEmailPage.cs  # Email verification page
├── Fixtures/               # xUnit fixtures
│   └── BrowserFixture.cs   # Browser lifecycle management
├── Configuration/          # Configuration classes
│   └── TestConfiguration.cs # Test settings loader
├── Utilities/              # Helper utilities
│   └── TestDataGenerator.cs # Test data generation
├── appsettings.json        # Test configuration
└── InsightForge.E2E.Tests.csproj
```

## Prerequisites

- .NET 8.0 or later
- The Insight Forge frontend running on http://localhost:5173
- The Insight Forge backend running and configured

## Configuration

Configuration is read from `appsettings.json`:

```json
{
  "Playwright": {
    "BaseUrl": "http://localhost:5173",
    "HeadlessMode": true,
    "Timeout": 30000,
    "NavigationTimeout": 30000,
    "ScreenshotOnFailure": true,
    "SlowMo": 0
  },
  "TestData": {
    "ValidEmail": "testuser@example.com",
    "ValidPassword": "SecurePassword123!"
  }
}
```

Override settings with environment variables:
- `Playwright:BaseUrl=http://localhost:3000`
- `Playwright:HeadlessMode=false`

## Running Tests

### Run all tests

```bash
cd src/testing
dotnet test
```

### Run specific test class

```bash
dotnet test --filter "ClassName=SignupTests"
```

### Run specific test method

```bash
dotnet test --filter "Name=Should_Successfully_Register_With_Valid_Credentials"
```

### Run tests with detailed output

```bash
dotnet test -v normal
```

### Run tests in headless mode (default)

```bash
dotnet test
```

### Run tests with UI visible

Set `Playwright:HeadlessMode` to `false` in `appsettings.json` or via environment variable:

```bash
Playwright__HeadlessMode=false dotnet test
```

## Page Object Model

All page interactions go through page objects that encapsulate UI details and expose business operations.

### Example

```csharp
// Good - Business operation
await registerPage.RegisterAsync(email, password);

// Bad - Low-level UI interaction in test
await page.FillAsync("#email", email);
await page.FillAsync("#password", password);
await page.ClickAsync("button[type='submit']");
```

## Test Data

Tests generate unique test data to ensure isolation:

```csharp
var uniqueEmail = TestDataGenerator.GenerateUniqueEmail();
var password = TestDataGenerator.GenerateStrongPassword();
```

## Best Practices

1. **One test = one business scenario** - Keep tests focused
2. **Use Playwright assertions** - Don't use manual polling
3. **Wait for UI state** - Use `WaitForLoadStateAsync` and `Expect`
4. **Generate unique data** - Use `TestDataGenerator` for test data
5. **Fresh browser context** - Each test gets a fresh context
6. **No hardcoded URLs** - Use configuration
7. **Descriptive test names** - Should_<Action>_When_<Condition>

## Adding New Tests

1. Create a new test class inheriting from the appropriate test base
2. Use the page objects to interact with the UI
3. Follow the Arrange-Act-Assert pattern
4. Use descriptive test method names
5. Generate unique test data

### Example

```csharp
[Fact]
public async Task Should_Display_Error_When_Email_Is_Invalid()
{
    // Arrange
    await _registerPage.GotoRegisterPageAsync();
    var password = TestDataGenerator.GenerateStrongPassword();

    // Act
    await _registerPage.EnterEmailAsync("invalid-email");
    await _registerPage.EnterPasswordAsync(password);
    await _registerPage.EnterConfirmPasswordAsync(password);
    await _registerPage.AgreeToTermsAsync();
    await _registerPage.ClickSubmitButtonAsync();

    // Assert
    var errorMessage = await _registerPage.GetErrorMessageAsync();
    Assert.Contains("email", errorMessage, StringComparison.OrdinalIgnoreCase);
}
```

## Debugging Tests

### Enable UI (headless = false)

```json
{
  "Playwright": {
    "HeadlessMode": false
  }
}
```

### Slow down execution

```json
{
  "Playwright": {
    "SlowMo": 1000
  }
}
```

### Use Playwright Inspector

```bash
PWDEBUG=1 dotnet test
```

## Troubleshooting

### Tests timeout

Increase the `Timeout` value in `appsettings.json`:

```json
{
  "Playwright": {
    "Timeout": 60000
  }
}
```

### Browser context issues

Ensure the application is running on the configured `BaseUrl`.

### Element not found

Check:
1. Application is running
2. Navigation succeeded (check URL)
3. Element locator is correct
4. Element is visible and in DOM

## CI/CD Integration

To run tests in CI/CD:

```bash
cd src/testing
dotnet test --logger "console;verbosity=minimal" --results-directory "./test-results" --collect:"XPlat Code Coverage"
```

## Contributing

When adding new tests:
1. Follow the Page Object Model pattern
2. Use descriptive test names
3. Generate unique test data
4. Keep tests isolated and independent
5. Avoid hardcoding values
6. Document complex test scenarios
