Insight-forge: Copilot Instruction File
=====================================

Purpose
-------
This file describes how Copilot (and contributors) should generate, modify, and reason about code for the Insight-forge server-side application.



High-level requirements
-----------------------
- App name: Insight-forge
- Server: C# Web API targeting .NET 10
- The server is responsible for orchestration: calling agents, creating/ managing agents, routing requests, business logic.
- Agent implementation details (provider-specific code, API contracts, credentials) must be strictly separated from business logic and orchestration.
- Initial provider: Gemini. Design must allow adding other agent providers without changing business logic.

Architecture and folder guidance
-------------------------------
- src/InsightForge.Api - Web API controllers, DTOs, request/response mapping, routing.
- src/InsightForge.Core - Business logic, domain models, services that do NOT reference provider SDKs.
- src/InsightForge.Agents - Abstractions and provider implementations.
  - src/InsightForge.Agents.Abstractions - IAgentClient, IAgentFactory, models used across providers
  - src/InsightForge.Agents.Gemini - Gemini-specific client, transport, configuration

Design principles
-----------------
- Define clear interfaces in Agents.Abstractions (e.g., IAgentClient, IAgentManager, IAgentCredentialsStore). Business services depend only on these interfaces.
- Implement provider-specific logic in separate projects (e.g., Agents.Gemini) that implement the abstractions.
- Register abstractions with dependency injection in the API startup using configuration to select provider implementation.
- Keep configuration (API keys, endpoints) out of source; use configuration providers or secret stores.
- Keep minimal surface area: map provider responses to shared domain models inside Agents layer before returning to Core.

Example interfaces (guidance)
----------------------------
- public interface IAgentClient
  - Task<AgentResponse> SendAsync(AgentRequest request, CancellationToken ct);
- public interface IAgentManager
  - Task<ManagedAgent> CreateAgentAsync(AgentSpec spec, CancellationToken ct);
  - Task<AgentStatus> GetStatusAsync(string agentId, CancellationToken ct);

Dependency injection and selection
---------------------------------
- Use configuration to pick provider at startup, e.g.:
  - if (config["Agents:Provider"] == "Gemini") services.AddSingleton<IAgentClient, GeminiAgentClient>();
- Avoid new-ing provider clients inside Core services.

Testing and validation
----------------------
- Unit-test Core services by mocking IAgentClient / IAgentManager.
- Provide integration tests for each provider package in Agents.* projects.

Extensibility rules
-------------------
- Adding a new provider: create new project Agents.<Provider>, implement Agents.Abstractions interfaces, add DI registration and configuration, write integration tests. No Core changes.

Security and secrets
--------------------
- Do not commit keys. Use environment variables or secret stores.

Coding style and PR expectations
------------------------------
- Keep changes minimal and focused.
- Add unit tests for behavior changes.
- Document new public interfaces in README in the package folder.

Contact and ownership
---------------------
If unclear, prefer minimal, well-tested abstractions over broad flexible designs.
