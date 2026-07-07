# Insight Forge - MVP Requirements

## Overview

**Insight Forge** is an AI-powered content generation platform that leverages **Google Gemini Managed Agents** to automate research, generate technical blog articles, and summarize multiple reference sources.

The MVP focuses exclusively on **Google Gemini**. The architecture will be Gemini-specific while keeping the code organized to simplify future support for additional AI providers.

---

# Functional Requirements

| ID | Module | Requirement | Priority |
|----|--------|-------------|----------|
| **IF-001** | User Management | The system shall allow administrators to create user accounts. | High |
| IF-002 | User Management | The system shall optionally allow users to self-register through configuration. | Medium |
| IF-003 | User Management | The system shall allow administrators to update user information. | High |
| IF-004 | User Management | The system shall allow administrators to enable or disable user accounts. | High |
| IF-005 | User Management | The system shall allow administrators to delete user accounts. | Medium |
| IF-006 | User Management | The system shall assign one or more roles to each user. | High |
| IF-007 | User Management | The system shall allow administrators to change user roles. | High |
| IF-008 | User Management | The system shall maintain user profile information including name, email, role and status. | High |
| IF-009 | User Management | The system shall record audit information including creation date and last login. | Medium |
| **IF-010** | Authentication | The system shall authenticate users before allowing access to protected resources. | High |
| IF-011 | Authentication | The system shall support JWT Bearer authentication. | High |
| IF-012 | Authentication | The system shall issue JWT access tokens after successful authentication. | High |
| IF-013 | Authentication | The system shall support refresh tokens. | Medium |
| IF-014 | Authentication | The system shall securely store passwords using industry-standard hashing. | High |
| IF-015 | Authentication | The system shall support password changes. | Medium |
| IF-016 | Authentication | The system shall support password reset functionality. | Medium |
| IF-017 | Authentication | The system shall enforce role-based authorization (RBAC). | High |
| IF-018 | Authentication | The system shall restrict administrative functionality to authorized users. | High |
| **IF-019** | Blog Generation | The system shall allow users to submit a topic for blog generation. | High |
| IF-020 | Blog Generation | The system shall accept optional parameters such as audience, tone, target length and SEO keywords. | Medium |
| IF-021 | Blog Generation | The system shall initiate a research workflow before generating a blog. | High |
| IF-022 | Blog Generation | The system shall generate a complete blog article in Markdown format. | High |
| IF-023 | Blog Generation | Generated blogs shall include a title, summary, table of contents, headings and references. | High |
| IF-024 | Blog Generation | The system shall include code snippets where appropriate. | Medium |
| IF-025 | Blog Generation | The system shall include Mermaid diagrams where appropriate. | Medium |
| IF-026 | Blog Generation | The system shall review generated blogs for grammar, formatting and completeness before returning them. | Medium |
| **IF-027** | Research | The system shall use a Gemini Managed Agent to research the requested topic. | High |
| IF-028 | Research | Research shall include facts, key concepts, references and supporting information where available. | High |
| IF-029 | Research | Research results shall be made available to the blog generation workflow. | High |
| **IF-030** | Summarization | The system shall allow users to submit multiple references for summarization. | High |
| IF-031 | Summarization | Supported references shall include URLs, Markdown, PDF, Microsoft Word documents and plain text. | High |
| IF-032 | Summarization | The system shall extract textual content from all supplied references. | High |
| IF-033 | Summarization | The system shall generate a consolidated summary of all supplied references. | High |
| IF-034 | Summarization | Generated summaries shall include an executive summary, key findings and referenced sources. | High |
| **IF-035** | Agent Management | The system shall automatically create a Gemini Managed Agent when one does not already exist. | High |
| IF-036 | Agent Management | Existing Managed Agents shall be reused for subsequent requests. | High |
| IF-037 | Agent Management | Managed Agent identifiers shall be persisted for future use. | High |
| IF-038 | Agent Management | If a Managed Agent is deleted externally, the system shall automatically recreate it. | High |
| IF-039 | Agent Management | Managed Agents shall be initialized using predefined instructions stored within the application. | High |
| **IF-040** | Prompt Management | The system shall maintain prompt templates for Research, Blog Writing, Summarization and Review. | High |
| IF-041 | Prompt Management | Prompt templates shall support versioning. | Medium |
| **IF-042** | Gemini Integration | The system shall use the official Gemini SDK for all AI interactions. | High |
| IF-043 | Gemini Integration | The Gemini model shall be configurable through application settings. | High |
| IF-044 | Gemini Integration | The system shall support streamed AI responses where supported by Gemini. | Medium |
| IF-045 | Gemini Integration | The system shall support Gemini function/tool calling when required. | Medium |
| **IF-046** | Content Management | The system shall store generated blog articles. | High |
| IF-047 | Content Management | The system shall store generated summaries. | High |
| IF-048 | Content Management | The system shall store research outputs. | Medium |
| **IF-049** | Configuration | The system shall securely store the Gemini API key. | High |
| IF-050 | Configuration | The system shall allow configuration of the default Gemini model. | High |
| IF-051 | Configuration | The system shall allow configuration of generation parameters including temperature and token limits. | Medium |
| **IF-052** | Logging | The system shall log API requests, responses, execution duration and errors. | High |
| IF-053 | Logging | The system shall log authentication and administrative actions. | High |
| **IF-054** | Monitoring | The system shall capture Gemini token usage. | Medium |
| IF-055 | Monitoring | The system shall capture Gemini API latency and request failures. | Medium |
| **IF-056** | Security | Sensitive configuration values shall be encrypted at rest. | High |
| IF-057 | Security | User credentials and Gemini API keys shall never be exposed through logs or API responses. | High |
| **IF-058** | Reliability | The system shall automatically retry transient Gemini API failures. | High |
| IF-059 | Reliability | The system shall return meaningful error messages when processing fails. | High |
| **IF-060** | Architecture | The MVP shall be implemented specifically for Gemini without a provider abstraction layer. | High |
| IF-061 | Architecture | Business workflows shall be isolated from Gemini SDK integration to simplify future support for additional providers. | High |

---

# Non-Functional Requirements

## Performance

- Blog generation should begin streaming results as soon as content becomes available.
- The system should efficiently process multiple reference documents.
- API response times should remain consistent under normal operating load.

## Reliability

- Automatically retry transient Gemini API failures.
- Automatically recreate missing Managed Agents.
- Gracefully handle partial failures during workflow execution.

## Security

- Encrypt sensitive configuration values.
- Store user passwords using secure hashing.
- Never expose secrets or credentials through logs or API responses.

## Maintainability

- Separate business workflows from Gemini SDK implementation.
- Keep prompt templates independent of application logic.
- Organize services using clear separation of concerns.

## Observability

- Capture structured logs.
- Track request duration.
- Track token usage.
- Track Gemini API latency.
- Log workflow execution history.

---

# Future Scope

| ID | Requirement |
|----|-------------|
| FUT-001 | Support multiple AI providers (OpenAI, Claude, Azure OpenAI, Ollama, etc.) |
| FUT-002 | Introduce a provider abstraction layer |
| FUT-003 | User-defined Managed Agents |
| FUT-004 | Visual workflow designer |
| FUT-005 | Prompt editor |
| FUT-006 | Scheduled blog generation |
| FUT-007 | Knowledge base integration (GitHub, Confluence, SharePoint, etc.) |
| FUT-008 | Multi-agent collaboration |
| FUT-009 | AI model routing |
| FUT-010 | Cost analytics |
| FUT-011 | Semantic search and vector knowledge base |
| FUT-012 | Plugin architecture for custom tools and workflows |

---

# MVP Architecture Principles

- Gemini is the only supported AI provider.
- Managed Agents are created automatically on first use.
- Managed Agents are persisted and reused.
- Workflows orchestrate business logic.
- Prompt templates are versioned and reusable.
- Business services are independent of Gemini SDK implementation.
- The architecture should allow future introduction of additional AI providers with minimal changes.