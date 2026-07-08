# Playwright Automation Guidelines

## Purpose

Generate high-quality end-to-end UI automation tests for the Insight Forge application using Playwright for .NET.

Tests should be maintainable, readable, deterministic, and easy to extend.

---

# Technology

- Language: C#
- Test Framework: xUnit
- UI Automation: Microsoft Playwright
- Pattern: Page Object Model (POM)
- Assertions: Playwright Assertions
- Async: Always use async/await

---

# General Principles

- NEVER generate recorded Playwright code.
- NEVER generate long procedural tests.
- Prefer reusable page objects.
- Keep tests focused on one business scenario.
- Generate production-quality code.
- Tests must be deterministic.
- Avoid arbitrary delays.

DO NOT use:

```
WaitForTimeout()
Thread.Sleep()
Task.Delay()
```

Use Playwright waits instead.

---

# Test Structure

Each test should follow:

Arrange

Act

Assert

Keep assertions grouped at the end whenever possible.

Example

```
Login
Create Insight
Verify Insight appears
```

NOT

```
Login
Assert

Create
Assert

Navigate
Assert
```

unless intermediate validation is required.

---

# Naming

Test classes

```
LoginTests
InsightCreationTests
DashboardTests
```

Test methods

```
Should_Login_With_Valid_Credentials()

Should_Show_Error_For_Invalid_Login()

Should_Create_New_Insight()

Should_Delete_Report()
```

Use descriptive names.

---

# Page Object Model

Every screen must have its own Page Object.

Example

```
LoginPage

DashboardPage

InsightPage

ReportPage

SettingsPage
```

Page Objects should

- expose business operations
- expose element locators only when necessary
- hide implementation details

Good

```
await loginPage.LoginAsync(user);
```

Bad

```
await page.GetByRole(...)

await page.ClickAsync(...)

await page.FillAsync(...)
```

inside test methods.

---

# Locators

Prefer the following order:

1. data-testid
2. getByRole
3. getByLabel
4. getByPlaceholder
5. getByText
6. CSS selectors
7. XPath (avoid)

Never generate brittle CSS selectors.

---

# Assertions

Use Playwright assertions.

Example

```
await Expect(page).ToHaveURLAsync(...)

await Expect(locator).ToContainTextAsync(...)

await Expect(locator).ToBeVisibleAsync()
```

Avoid manual polling.

---

# Waiting

Always wait for UI state instead of sleeping.

Prefer

```
WaitForURLAsync

WaitForLoadStateAsync

Expect(locator).ToBeVisibleAsync()

Expect(locator).ToBeHiddenAsync()

Expect(locator).ToContainTextAsync()
```

---

# Authentication

Authentication should be reusable.

Prefer:

- authenticated browser state
- shared login helper
- fixture

Do not repeat login logic in every test.

---

# Test Isolation

Every test must be independent.

Tests should

- create their own data
- clean up their own data when needed
- never depend on execution order

---

# Test Data

Never hardcode reusable identifiers.

Generate unique names.

Example

```
Insight
Insight-{Guid.NewGuid()}
```

Prefer helper factories.

---

# Browser Context

Each test should use a fresh BrowserContext unless intentionally testing persistence.

Avoid shared mutable state.

---

# Error Handling

Do not swallow exceptions.

Let Playwright produce meaningful failure messages.

---

# Screenshots

Capture screenshots only on failure.

Use Playwright tracing where appropriate.

Enable

- Trace
- Video (optional)
- Screenshot on failure

---

# File Organization

Organize tests by feature.

Example

```
Tests/
    Authentication/
    Dashboard/
    Insights/
    Reports/
    Settings/

Pages/

Components/

Fixtures/

Utilities/

TestData/
```

---

# Components

Reusable UI widgets should become Components instead of Page Objects.

Examples

```
NavigationMenu

TopToolbar

ConfirmationDialog

ToastNotification

DataGrid

Pagination
```

---

# Fixtures

Use xUnit fixtures for

- Browser
- Authentication
- Test Server
- Shared configuration

Avoid duplicate setup code.

---

# Configuration

Read configuration from

- appsettings.json
- environment variables

Never hardcode

- URLs
- passwords
- API keys

---

# Accessibility

Whenever practical, use semantic locators.

Example

```
GetByRole

GetByLabel
```

This improves test stability.

---

# Test Quality

Every generated test should be

- deterministic
- readable
- maintainable
- isolated
- reusable

The generated code should resemble code written by an experienced automation engineer.

---

# AI Expectations

When generating tests:

- Reuse existing Page Objects whenever possible.
- Do not duplicate helper methods.
- Extend existing page objects instead of creating similar ones.
- If an element already exists in a page object, reuse it.
- Keep page objects small and cohesive.
- Ask questions if required UI behavior is ambiguous.
- Never assume element IDs, test IDs, routes, or page structure if they are not available.
- If the implementation is unclear, request the relevant Razor/Vue component or HTML before generating automation.
- Prefer business-readable test steps over low-level UI interactions.

---

# Insight Forge Conventions

When writing automation:

- Treat Insights, Reports, Dashboards, and Settings as business concepts.
- Validate user-visible behavior rather than implementation details.
- Verify success messages, validation messages, and navigation where applicable.
- Prefer assertions based on visible UI state instead of internal values.
- Keep tests resilient to UI layout changes by relying on semantic locators and data-testid attributes.
- When new functionality is added, first determine whether an existing Page Object or Component should be extended before creating new classes.
