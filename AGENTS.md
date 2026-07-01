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

# Solution Architecture

The solution follows Clean Architecture.

```
Presentation
        │
Application
        │
Domain
        │
Infrastructure
```

Dependencies always point inward.

The Domain layer must never depend on Infrastructure.

Business logic belongs in Application.

Infrastructure contains external integrations.

Presentation contains only transport concerns.

---

# Repository Layout

```
src/server : Web API
src/client : Client 

```

# Definition of Done

A feature is complete when:

- implementation is complete
- tests pass
- public APIs are documented
- architecture remains consistent
- no unnecessary coupling is introduced