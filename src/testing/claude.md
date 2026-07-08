# Playwright Automation Guidelines

## Purpose

Generate production-quality end-to-end UI automation tests for Insight Forge using Playwright for .NET and MSTest.

The primary goals are:

- Readability
- Maintainability
- Reliability
- Fast execution
- Deterministic results

---

# Technology Stack

- Language: C#
- Framework: MSTest
- UI Automation: Microsoft Playwright
- Pattern: Page Object Model (POM)
- Assertions: Microsoft.Playwright.Assertions
- Async Programming: async/await

---

# General Principles

Always generate clean, maintainable automation code.

DO:

- Follow SOLID principles.
- Keep tests independent.
- Keep page objects focused.
- Reuse existing components.
- Prefer composition over duplication.
- Write business-readable tests.

DO NOT:

- Generate Playwright Recorder code.
- Use procedural automation.
- Duplicate locators.
- Duplicate helper methods.
- Swallow exceptions.

---

# Test Structure

Each test should follow Arrange → Act → Assert.

Example:

```text
Arrange
    Login as Administrator

Act
    Create a new Insight

Assert
    Verify the Insight appears in the grid
```

Avoid mixing assertions throughout the test unless validating an intermediate step.

---

# MSTest Conventions

Use:

- `[TestClass]`
- `[TestMethod]`
- `[TestInitialize]`
- `[TestCleanup]`
- `[ClassInitialize]`
- `[ClassCleanup]`

Prefer asynchronous test methods.

Example:

```csharp
[TestMethod]
public async Task Should_Create_New_Insight()
{
}
```

Never use synchronous Playwright APIs.

---

# Naming Conventions

## Test Classes

```
LoginTests
DashboardTests
InsightTests
ReportTests
SettingsTests
```

## Test Methods

```
Should_Login_With_Valid_Credentials

Should_Reject_Invalid_Login

Should_Create_New_Insight

Should_Delete_Report
```

Names should describe expected behaviour.

---

# Page Object Model

Every page must have its own Page Object.

Example:

```
LoginPage
DashboardPage
InsightPage
ReportPage
SettingsPage
```

Page Objects should expose business operations.

Good:

```csharp
await loginPage.LoginAsync(user);
```

Bad:

```csharp
await page.GetByRole(...).ClickAsync();
await page.FillAsync(...);
```

Low-level Playwright code belongs inside Page Objects.

---

# Component Objects

Reusable UI widgets should be Component Objects.

Examples:

```
NavigationMenu
TopToolbar
DataGrid
ToastNotification
ConfirmationDialog
Pagination
```

Components may be shared across multiple pages.

---

# Locators

Use the following priority:

1. data-testid
2. GetByRole
3. GetByLabel
4. GetByPlaceholder
5. GetByText
6. CSS selectors

Avoid XPath.

Never use fragile selectors based on nested div structures.

---

# Waiting Strategy

Never use:

```csharp
Thread.Sleep()

Task.Delay()

WaitForTimeoutAsync()
```

Instead use:

```csharp
WaitForURLAsync()

WaitForLoadStateAsync()

Expect(locator).ToBeVisibleAsync()

Expect(locator).ToBeHiddenAsync()

Expect(locator).ToContainTextAsync()

Expect(locator).ToHaveValueAsync()
```

Always wait for application state.

---

# Assertions

Use Playwright Assertions.

Preferred:

```csharp
await Expect(locator).ToBeVisibleAsync();

await Expect(locator).ToContainTextAsync();

await Expect(page).ToHaveURLAsync(...);
```

Avoid manual polling.

---

# Authentication

Authentication should be reusable.

Prefer:

- Shared login helper
- StorageState
- Authenticated browser context

Avoid logging in inside every test.

---

# Test Isolation

Every test must:

- Create its own data
- Clean up its own data where appropriate
- Never depend on another test

Tests must execute successfully in any order.

---

# Test Data

Never hardcode reusable identifiers.

Generate unique data.

Example:

```csharp
var insightName = $"Insight-{Guid.NewGuid()}";
```

Prefer dedicated TestData helper classes.

---

# Browser Context

Each test should execute in a fresh BrowserContext unless testing persistence.

Never share mutable state between tests.

---

# Fixtures

Create reusable base classes for common setup.

Examples:

```
PlaywrightTestBase

AuthenticatedTestBase

AdminTestBase
```

Avoid duplicated initialization logic.

---

# Project Structure

```
Playwright

    Pages/

    Components/

    Tests/

        Authentication/

        Dashboard/

        Insights/

        Reports/

        Settings/

    Fixtures/

    Utilities/

    TestData/

    Helpers/
```

Organize tests by feature.

---

# Configuration

Read configuration from:

- appsettings.json
- appsettings.Development.json
- Environment Variables

Never hardcode:

- URLs
- Credentials
- API Keys

---

# Screenshots & Tracing

Enable:

- Trace on failure
- Screenshot on failure

Video recording should be configurable.

Do not capture screenshots during successful execution unless explicitly required.

---

# Accessibility

Prefer semantic locators.

Use:

```csharp
GetByRole()

GetByLabel()
```

This improves test stability.

---

# Error Handling

Allow Playwright exceptions to propagate naturally.

Do not suppress failures.

Avoid unnecessary try/catch blocks.

---

# Code Quality

Generated code should:

- Follow existing project conventions.
- Reuse existing Page Objects.
- Extend existing Components.
- Avoid duplication.
- Use descriptive variable names.
- Keep methods small.

---

# AI Expectations

Before generating code:

- Search for existing Page Objects.
- Search for existing Components.
- Search for helper methods.
- Search for similar tests.

Reuse existing implementations whenever possible.

Never create duplicate abstractions.

If required information is missing:

- Ask for the page HTML.
- Ask for Razor/Vue component markup.
- Ask for screenshots if locator selection is ambiguous.

Do not invent:

- data-testid values
- URLs
- CSS selectors
- DOM structure

---

# Insight Forge Conventions

Treat the application using business terminology.

Examples:

- Insight
- Dashboard
- Report
- Widget
- User
- Settings

Tests should validate business behaviour rather than implementation details.

Verify:

- Success messages
- Validation messages
- Navigation
- User-visible state
- Data persistence where applicable

Avoid asserting implementation-specific details unless explicitly requested.
