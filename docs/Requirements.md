# Insight Forge - MVP Requirements

## Overview

**Insight Forge** is an AI-powered content generation platform that leverages **Google Gemini Managed Agents** to automate research, generate technical blog articles, and summarize information from multiple reference sources.

The MVP supports **Google Gemini only**. The architecture shall be Gemini-specific while maintaining a clean separation between business workflows and Gemini SDK implementation to simplify future support for additional AI providers.

---

# Functional Requirements

## Account Management

| ID         | Requirement                                                                                   | Priority | Status |
| ---------- | --------------------------------------------------------------------------------------------- | -------- | ------ |
| **IF-100** | The system shall allow users to register an account using an email address and password.      | High     | ✅      |
| IF-101     | The system shall ensure that email addresses are unique.                                      | High     | ✅      |
| IF-102     | The system shall require users to verify their email address before activating their account. | Medium   | ✅      |
| IF-103     | The system shall allow users to sign in using their registered credentials.                   | High     | ✅      |
| IF-104     | The system shall allow users to sign out of the application.                                  | High     | ⬜      |
| IF-105     | The system shall allow users to update their profile information.                             | Medium   | ⬜      |
| IF-106     | The system shall allow users to change their email address after successful verification.     | Medium   | ⬜      |
| IF-107     | The system shall allow users to deactivate their own account.                                 | Low      | ⬜      |
| IF-108     | The system shall maintain account status (Pending Verification, Active, Disabled).            | Medium   | ⬜      |
| IF-109     | The system shall record account creation date and last login date.                            | Medium   | ⬜      |


---

## Authentication

| ID         | Requirement                                                                                   | Priority | Status |
| ---------- | --------------------------------------------------------------------------------------------- | :------: | :----: |
| **IF-200** | The system shall authenticate registered users before allowing access to protected resources. |   High   |    ✅   |
| IF-201     | The system shall support JWT Bearer authentication.                                           |   High   |    ✅   |
| IF-202     | The system shall issue JWT access tokens after successful authentication.                     |   High   |    ✅   |
| IF-203     | The system shall support refresh tokens.                                                      |  Medium  |    ✅   |
| IF-204     | The system shall securely store passwords using industry-standard hashing algorithms.         |   High   |    ✅   |
| IF-205     | The system shall allow users to change their password.                                        |  Medium  |    ⬜   |
| IF-206     | The system shall allow users to reset forgotten passwords using email verification.           |  Medium  |    ⬜   |
| IF-207     | The system shall invalidate refresh tokens upon logout.                                       |  Medium  |    ⬜   |
| IF-208     | The system shall reject requests containing expired or invalid authentication tokens.         |   High   |    ⬜   |


---

## Blog Generation

| ID         | Requirement                                                                                              | Priority |
| ---------- | -------------------------------------------------------------------------------------------------------- | -------- |
| **IF-300** | The system shall allow users to submit a topic for blog generation.                                      | High     |
| IF-301     | The system shall allow users to specify the intended audience.                                           | Medium   |
| IF-302     | The system shall allow users to specify the writing style or tone.                                       | Medium   |
| IF-303     | The system shall allow users to specify the desired article length.                                      | Medium   |
| IF-304     | The system shall allow users to specify SEO keywords.                                                    | Medium   |
| IF-305     | The system shall automatically initiate a research workflow before generating the blog.                  | High     |
| IF-306     | The system shall generate a complete blog article in Markdown format.                                    | High     |
| IF-307     | The generated blog shall include a title.                                                                | High     |
| IF-308     | The generated blog shall include an executive summary.                                                   | High     |
| IF-309     | The generated blog shall include a table of contents.                                                    | High     |
| IF-310     | The generated blog shall include appropriate section headings.                                           | High     |
| IF-311     | The generated blog shall include references used during research.                                        | High     |
| IF-312     | The generated blog shall include citations where appropriate.                                            | High     |
| IF-313     | The system shall generate code snippets when appropriate.                                                | Medium   |
| IF-314     | The system shall generate Mermaid diagrams when appropriate.                                             | Medium   |
| IF-315     | The system shall review generated content for grammar, formatting, and completeness before returning it. | Medium   |
| IF-316     | The generated blog shall be stored for future retrieval.                                                 | Medium   |

---

## Research

| ID         | Requirement                                                                  | Priority |
| ---------- | ---------------------------------------------------------------------------- | -------- |
| **IF-400** | The system shall use a Gemini Managed Agent to research the requested topic. | High     |
| IF-401     | The research shall include relevant facts.                                   | High     |
| IF-402     | The research shall identify key concepts.                                    | High     |
| IF-403     | The research shall identify supporting references.                           | High     |
| IF-404     | The research shall include statistics where available.                       | Medium   |
| IF-405     | The research shall identify related technologies where applicable.           | Medium   |
| IF-406     | Research results shall be supplied to the blog generation workflow.          | High     |
| IF-407     | Research results shall be stored for future reference.                       | Medium   |

---

## Reference Summarization

| ID         | Requirement                                                                   | Priority |
| ---------- | ----------------------------------------------------------------------------- | -------- |
| **IF-500** | The system shall allow users to submit multiple references for summarization. | High     |
| IF-501     | The system shall support URL references.                                      | High     |
| IF-502     | The system shall support Markdown documents.                                  | High     |
| IF-503     | The system shall support PDF documents.                                       | High     |
| IF-504     | The system shall support Microsoft Word documents.                            | High     |
| IF-505     | The system shall support plain text documents.                                | High     |
| IF-506     | The system shall extract textual content from all supplied references.        | High     |
| IF-507     | The system shall generate a consolidated summary.                             | High     |
| IF-508     | The generated summary shall include an executive summary.                     | High     |
| IF-509     | The generated summary shall include key findings.                             | High     |
| IF-510     | The generated summary shall include the references used.                      | High     |
| IF-511     | The generated summary shall be stored for future retrieval.                   | Medium   |

---

## Agent Management

| ID         | Requirement                                                                                                 | Priority |
| ---------- | ----------------------------------------------------------------------------------------------------------- | -------- |
| **IF-600** | The system shall automatically create a Gemini Managed Agent when one does not already exist.               | High     |
| IF-601     | Existing Managed Agents shall be reused for subsequent requests.                                            | High     |
| IF-602     | Managed Agent identifiers shall be persisted for future requests.                                           | High     |
| IF-603     | If a Managed Agent is deleted externally, the system shall automatically recreate it.                       | High     |
| IF-604     | Managed Agents shall be initialized using predefined instructions stored within the application.            | High     |
| IF-605     | The system shall maintain separate Managed Agents for Research, Blog Generation, Summarization, and Review. | High     |

---

## Prompt Management

| ID         | Requirement                                                             | Priority |
| ---------- | ----------------------------------------------------------------------- | -------- |
| **IF-700** | The system shall maintain prompt templates for all AI workflows.        | High     |
| IF-701     | Prompt templates shall support versioning.                              | Medium   |
| IF-702     | Prompt templates shall be reusable across workflows.                    | High     |
| IF-703     | Prompt templates shall be maintained independently of application code. | High     |

---

## Gemini Integration

| ID         | Requirement                                                                  | Priority |
| ---------- | ---------------------------------------------------------------------------- | -------- |
| **IF-800** | The system shall use the official Google Gemini SDK for all AI interactions. | High     |
| IF-801     | The Gemini model shall be configurable through application settings.         | High     |
| IF-802     | The system shall support streamed AI responses where supported.              | Medium   |
| IF-803     | The system shall support Gemini function and tool calling when required.     | Medium   |

---

## Content Management

| ID         | Requirement                                                                   | Priority |
| ---------- | ----------------------------------------------------------------------------- | -------- |
| **IF-900** | The system shall maintain a history of generated blog articles for each user. | High     |
| IF-901     | The system shall maintain a history of generated summaries for each user.     | High     |
| IF-902     | Users shall be able to retrieve previously generated content.                 | High     |
| IF-903     | Users shall be able to delete previously generated content.                   | Medium   |

---

## Configuration

| ID          | Requirement                                                                                                                      | Priority |
| ----------- | -------------------------------------------------------------------------------------------------------------------------------- | -------- |
| **IF-1000** | The system shall securely store the Gemini API key.                                                                              | High     |
| IF-1001     | The system shall allow configuration of the default Gemini model.                                                                | High     |
| IF-1002     | The system shall allow configuration of generation parameters including temperature, maximum output tokens, and response format. | Medium   |

---

## Logging & Monitoring

| ID          | Requirement                                       | Priority |
| ----------- | ------------------------------------------------- | -------- |
| **IF-1100** | The system shall log API requests and responses.  | High     |
| IF-1101     | The system shall log workflow execution duration. | High     |
| IF-1102     | The system shall log application errors.          | High     |
| IF-1103     | The system shall record Gemini token usage.       | Medium   |
| IF-1104     | The system shall record Gemini API latency.       | Medium   |
| IF-1105     | The system shall record Gemini API failures.      | Medium   |

---

## Security

| ID          | Requirement                                                            | Priority |
| ----------- | ---------------------------------------------------------------------- | -------- |
| **IF-1200** | Sensitive configuration values shall be encrypted at rest.             | High     |
| IF-1201     | User credentials shall never be exposed through logs or API responses. | High     |
| IF-1202     | Gemini API keys shall never be exposed through logs or API responses.  | High     |
| IF-1203     | All client communication shall occur over HTTPS.                       | High     |

---

## Reliability

| ID          | Requirement                                                                          | Priority |
| ----------- | ------------------------------------------------------------------------------------ | -------- |
| **IF-1300** | The system shall automatically retry transient Gemini API failures.                  | High     |
| IF-1301     | The system shall return meaningful error messages when processing fails.             | High     |
| IF-1302     | The system shall gracefully recover from workflow execution failures where possible. | Medium   |

---

## Architecture

| ID          | Requirement                                                                                                              | Priority |
| ----------- | ------------------------------------------------------------------------------------------------------------------------ | -------- |
| **IF-1400** | The MVP shall be implemented specifically for Google Gemini.                                                             | High     |
| IF-1401     | Business workflows shall be isolated from Gemini SDK implementation.                                                     | High     |
| IF-1402     | AI workflow orchestration shall be independent of Gemini SDK implementation.                                             | High     |
| IF-1403     | The architecture shall support future introduction of additional AI providers with minimal changes to business services. | Medium   |

---

# Non-Functional Requirements

## Performance

* Blog generation should begin streaming output as soon as content becomes available.
* Multiple reference documents should be processed efficiently.
* API response times should remain consistent under expected workloads.

## Reliability

* Automatically retry transient Gemini API failures.
* Automatically recreate deleted Managed Agents.
* Gracefully recover from workflow execution failures.

## Security

* Encrypt sensitive configuration values.
* Store passwords using secure hashing algorithms.
* Never expose credentials or secrets.

## Maintainability

* Separate business workflows from Gemini SDK implementation.
* Keep prompt templates independent from application code.
* Organize services using clear separation of responsibilities.

## Observability

* Capture structured logs.
* Record workflow execution history.
* Monitor Gemini token consumption.
* Monitor Gemini API latency.

---

# Future Scope

| ID      | Requirement                                                                |
| ------- | -------------------------------------------------------------------------- |
| FUT-100 | Support multiple AI providers (OpenAI, Claude, Azure OpenAI, Ollama, etc.) |
| FUT-101 | Introduce a provider abstraction layer                                     |
| FUT-102 | Allow users to create custom Managed Agents                                |
| FUT-103 | Visual workflow designer                                                   |
| FUT-104 | Prompt editor                                                              |
| FUT-105 | Scheduled blog generation                                                  |
| FUT-106 | Knowledge base integrations (GitHub, Confluence, SharePoint, etc.)         |
| FUT-107 | Multi-agent collaboration                                                  |
| FUT-108 | AI model routing                                                           |
| FUT-109 | Cost analytics                                                             |
| FUT-110 | Semantic search and vector knowledge base                                  |
| FUT-111 | Plugin architecture for custom tools and workflows                         |

---

# MVP Design Principles

* Google Gemini is the only supported AI provider.
* Managed Agents shall be created automatically on first use.
* Existing Managed Agents shall be reused whenever possible.
* Business workflows shall orchestrate all AI interactions.
* Prompt templates shall be reusable and versioned.
* Business logic shall be isolated from Gemini SDK implementation.
* The architecture shall be designed to support future AI providers with minimal changes.
