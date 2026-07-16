# Insight Forge

Insight Forge is an AI-powered content generation platform that uses **Google Gemini Managed Agents** to research topics, generate technical blog articles, and summarize reference material from multiple source types.

The platform separates business capabilities from AI provider implementations, so providers can be added, replaced, or upgraded with minimal impact on the rest of the system. The MVP targets Google Gemini only, but the architecture is designed to support additional providers (OpenAI, Claude, Azure OpenAI, etc.) in the future.

## Core Principles

- Provider agnostic
- Clean Architecture
- SOLID principles
- Dependency Injection
- Testability first
- Incremental evolution
- Security by default

## Features

- **Authentication** — Account registration with email verification, JWT bearer auth with refresh tokens, secure password hashing.
- **Blog Generation** — Submit a topic (with optional audience, tone, length, and SEO keywords) and generate a complete Markdown article with a title, executive summary, table of contents, sections, references, citations, and code/Mermaid diagrams where appropriate.
- **Research** — A Gemini Managed Agent researches the topic and supplies facts, key concepts, and references to the blog generation workflow.
- **Reference Summarization** — Summarize multiple references (URLs, Markdown, PDF, Word, plain text) into a consolidated summary with key findings.
- **Agent Management** — Managed Agents are created automatically on first use, reused across requests, and recreated automatically if deleted externally.
- **Streaming Status** — Long-running agent workflows report progress over Server-Sent Events (SSE) so the UI can show live status.

See [docs/Requirements.md](docs/Requirements.md) for the full MVP requirements and current status.

## Technology Stack

**Backend**
- C# / .NET 10, ASP.NET Core Web API
- Google Gemini SDK (Managed Agents)
- Azure Table Storage (Azurite emulator for local development)
- JWT Bearer authentication
- MSTest

**Frontend**
- Vue 3 (Composition API, `<script setup>`)
- TypeScript
- Vite
- Vue Router, Pinia
- Bootstrap 5
- Axios

## Repository Layout

```
src/server   : ASP.NET Core Web API (Insight.WebApi + Insight.Services.* class libraries)
src/client   : Vue 3 + TypeScript frontend
docs         : Requirements and supporting documentation
deploy       : One-click deployment / E2E test automation script
```

### Backend Solution Structure

`src/server/insight.webapi/insight.webapi.slnx`

- `insight.webapi` — Web API host (controllers, DI composition, startup)
- `insight.services.core` — Business logic and domain models
- `insight.services.interfaces` — Provider-agnostic abstractions (e.g. agent client/manager interfaces)
- `insight.services.services` — Application services
- `insight.services.ai.gemini` — Gemini-specific agent client implementation
- `insight.services.core.tests` — MSTest unit tests

## Getting Started

### Prerequisites

- .NET 10.0 SDK or higher
- Node.js (v18+) and npm
- A Google Gemini API key

### Quick Start

```powershell
# One-click: builds and starts backend + frontend
.\deploy\deploy.ps1 -SkipTests
```

### Manual Setup

```bash
# Backend
cd src/server/insight.webapi
dotnet restore
cd insight.webapi
dotnet run

# Frontend (separate terminal)
cd src/client
npm install
npm run dev
```

- Frontend: http://localhost:5173
- Backend API: http://localhost:5001/openapi (or the port configured in `appsettings.Development.json`)

Set your `GEMINI_API_KEY` in `appsettings.Development.json` before running blog generation, research, or summarization workflows.

For full setup details, Azurite configuration, CORS, and troubleshooting, see [DEVELOPMENT.md](DEVELOPMENT.md).

## Verification

**Backend**
```bash
dotnet build
dotnet test
```

**Frontend**
```bash
npm run build   # runs vue-tsc type checking, then builds
```

## Contributing

- Create a feature branch for any change; never commit directly to `main`.
- Commits follow [Conventional Commits](https://www.conventionalcommits.org/).
- Solutions must build without errors or warnings, and all tests must pass before merging.

See [CLAUDE.md](Claude.md) for detailed architecture guidelines, the definition of done, and mandatory verification steps.
