# Insight Forge

## Mission

Insight Forge is an AI orchestration platform designed to integrate with multiple Large Language Models (LLMs) through a provider-agnostic architecture.

The platform separates business capabilities from AI provider implementations, allowing providers to be added, replaced or upgraded with minimal impact on the rest of the system.

---

# Core Principles

- Provider agnostic
- Clean Architecture
- SOLID principles
- Dependency Injection
- Testability first
- Incremental evolution
- Security by default

---

# Repository Layout

```
src/server : Web API
src/client : Client
```

# Definition of Done

A feature is complete when:

- implementation is complete
- Solution can be build without errors or warnings
- tests pass
- public APIs are documented
- architecture remains consistent
- no unnecessary coupling is introduced

# Mandatory Verification

Every implementation MUST be verified before considering the task complete.

## Backend (.NET)

- Restore packages if needed.
- Build the affected project.
- Run the relevant MSTest tests.
- If API code changed:
  - Start the API.
  - Verify it starts without exceptions.
  - Verify the modified endpoint or functionality.
  - Stop the application after verification.

## Frontend (Vue)

- Run type checking.
- Run linting if configured.
- Build the application.
- If UI changed:
  - Launch the development server.
  - Open the affected page.
  - Verify the implemented behaviour.
  - Check browser console for errors.

## If verification cannot be completed

Do NOT assume the implementation works.
Explain:

- what was verified,
- what could not be verified,
- why it could not be be verified.

# Commit

- Allow commits should follow conventional commits format
- All solutions should compile and build without any error/warnings.
- All Unit tests should pass
