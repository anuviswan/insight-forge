# CLAUDE.md

# Insight Forge - Frontend

## Purpose

This project is the frontend for Insight Forge.

The application is built using Vue 3 and TypeScript and communicates with the backend through REST APIs.

Always prioritize maintainability, readability, and consistency over clever implementations.

---

# Technology Stack

- Vue 3
- TypeScript
- Vite
- Vue Router
- Pinia
- Bootstrap 5
- Axios (or the project's configured HTTP client)

Use the existing libraries already present in the project. Do not introduce new dependencies unless explicitly requested.

---

# General Rules

- Never assume requirements.
- Ask questions when requirements are unclear.
- Plan before making significant changes.
- Keep changes minimal and focused.
- Preserve existing functionality.
- Follow existing coding style and project conventions.
- Avoid unnecessary refactoring.
- Do not modify unrelated files.

---

# Vue Guidelines

## Components

- Use the Composition API.
- Use `<script setup lang="ts">`.
- Keep components focused on a single responsibility.
- Prefer composition over inheritance.
- Break large components into smaller reusable components.
- Avoid deeply nested component hierarchies.

## Props

- Strongly type all props.
- Use interfaces or type aliases.
- Avoid using `any`.

## Emits

- Define emits explicitly.
- Strongly type emitted events.

## Composables

Create composables for reusable logic such as:

- API access
- Form handling
- Authentication
- Search
- Filtering
- Pagination
- Common business logic

Avoid duplicating logic across components.

---

# TypeScript

Always prefer strong typing.

## Rules

- Never use `any` unless explicitly justified.
- Prefer interfaces for domain models.
- Prefer readonly where appropriate.
- Use enums or string unions consistently.
- Enable strict typing.
- Avoid type assertions unless absolutely necessary.

---

# State Management

Use Pinia for shared application state.

State should contain:

- User information
- Authentication
- Global application state
- Shared lookup data

Avoid placing temporary UI state inside global stores.

---

# Routing

- Keep routes organized.
- Use lazy loading where appropriate.
- Protect authenticated routes.
- Avoid duplicated route definitions.

---

# API Communication

- Use the project's configured HTTP client.
- Never call `fetch()` directly if Axios (or another client) is already configured.
- Centralize API configuration.
- Handle errors consistently.
- Use typed request and response models.
- Never duplicate endpoint definitions.

---

# Error Handling

Always provide user-friendly error messages.

Handle:

- Network failures
- Unauthorized responses
- Validation errors
- Unexpected server errors

Never silently swallow exceptions.

---

# Forms

- Validate before submitting.
- Display validation messages clearly.
- Disable submit buttons while requests are in progress.
- Prevent duplicate submissions.

---

# UI

Maintain a clean and consistent interface.

## Guidelines

- Prefer reusable components.
- Keep layouts responsive.
- Avoid inline styles.
- Keep CSS modular.
- Use Bootstrap utilities before creating custom CSS.

---

# Performance

- Lazy load pages where appropriate.
- Avoid unnecessary re-renders.
- Minimize watchers.
- Prefer computed properties over duplicated state.
- Debounce expensive operations such as search.

---

# Accessibility

Ensure all UI changes remain accessible.

- Use semantic HTML.
- Label form controls.
- Support keyboard navigation.
- Provide accessible names for buttons and icons.
- Maintain sufficient color contrast.

---

# Code Organization

Prefer the following structure:

```
src/
    api/
    assets/
    components/
        common/
        layout/
        feature/
    composables/
    models/
    router/
    services/
    stores/
    styles/
    utils/
    views/
```

Keep files close to the features they support.

---

# Naming Conventions

Components

- PascalCase

Examples

- UserCard.vue
- InsightList.vue
- LoginDialog.vue

Composables

- useAuthentication
- useInsights
- useSearch

Stores

- useUserStore
- useInsightStore

Interfaces

- User
- Insight
- SearchRequest

Types

- SortOption
- FilterType

Enums

- InsightStatus
- UserRole

---

# Testing

When adding functionality:

- Update existing tests where necessary.
- Add tests for new business logic.
- Do not remove tests without approval.

---

# Documentation

When introducing significant functionality:

- Update relevant documentation.
- Document important design decisions.
- Keep comments concise.
- Prefer self-explanatory code over excessive comments.

---

# Before Completing Any Task

Verify that:

- The application builds successfully.
- TypeScript compilation succeeds.
- No linting issues are introduced.
- Existing functionality remains unaffected.
- No unnecessary files were modified.

---

# What To Avoid

- Using `any`
- Large monolithic components
- Duplicate business logic
- Duplicate API calls
- Unused code
- Dead imports
- Console logging in production code
- Hardcoded URLs
- Hardcoded configuration values
- Magic strings when constants or enums are appropriate

---

# Decision Making

When multiple implementation approaches are possible:

1. Follow existing project conventions.
2. Choose the simplest maintainable solution.
3. Minimize breaking changes.
4. Prefer readability over cleverness.
5. Ask for clarification instead of making assumptions.
