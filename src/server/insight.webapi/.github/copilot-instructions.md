#  Github Copilot Instructions - Insight Forge API

## Project Overview

Insight Forge is a C# Web API built on .NET 10.

The API is responsible for:

Managing AI agents.
Loading agent definitions from disk.
Executing agent workflows.
Managing prompts and templates.
Exposing REST endpoints for clients.

The application is designed to support multiple AI providers. The business logic must remain independent of any specific provider.



## Platform
- Target framework: .NET 10
- Use latest C# version
- Nullable enabled
- Implicit usings enabled
- All I/O must be async/await
- Use Controllers by default (do not generate minimal APIs unless explicitly requested)

---
## Architecture
Follow Clean Architecture principles.
Layers:
 - API (Controllers)
 - Application (Use cases / Services)
 - Domain (Entities / Business rules)
 - Infrastructure (Azure Tables and Blob implementations)

Rules:
 - Controllers must be thin.
 - No business logic inside Controllers.
 - Application layer contains business rules.
 - Infrastructure layer handles Azure SDK interactions.
 - Controllers must not directly use Azure SDK clients.
 - Use dependency injection for all services.
 - Use primary constructor whereever applicable
 - No static service access.

---
---
## Error Handling
- Do not throw exceptions for expected domain errors.
- Use Result<T> pattern for application responses.
- Controllers convert Result<T> to proper HTTP responses.
- Implement global exception handling middleware.

---
## Logging
- Use ILogger<T>.
- Log warnings for recoverable issues.
- Log errors for unexpected failures.
- Never log sensitive data.

---
## Security
- Validate all inputs.
- Never store secrets in code.
- Use environment variables or Azure Key Vault.
- By default use [Authorize] for all endpoints
---
## Configuration
- Use strongly typed Options pattern.
- Do not access IConfiguration directly inside services.
- Secrets must come from secure configuration sources.
---

## Testing
- Application layer must be testable without Azure dependencies.
- Use interfaces for repositories and blob services.
- Do not mock concrete Azure SDK classes.
- use MS Tests and Moq
---
## Code Style
- Avoid complex LINQ chains.
- Keep methods concise and readable.
- Use meaningful names.
- No synchronous I/O.
---
## Forbidden Patterns
- No Azure SDK usage inside Controllers.
- No business logic in Infrastructure.
- No swallowing exceptions.
- No direct access to TableClient outside Infrastructure.
- No blob uploads without validation.



