<!-- Last reviewed: 2026-01-04 -->
<!-- ~300 tokens (hub file) -->

# CLAUDE.md

Guidance for Claude Code when working with this repository.

## Quick Reference

| Task | Command |
|------|---------|
| Build | `dotnet build --configuration Release` |
| Test | `dotnet test --configuration Release --verbosity normal` |
| Run Dev | `dotnet run --project src/Medley.AppHost` |
| Single Test | `dotnet test --filter "FullyQualifiedName~TestName"` |

## Project Overview

Medley is a **.NET 10 modular monolith** using C# 14, Blazor Interactive Auto, and Duende IdentityServer 7.4 with BFF pattern. Follows trunk-based development with squash merging.

## Architecture

```
src/
├── Medley.AppHost/           # Aspire orchestration
├── Medley.Web/               # Blazor UI + BFF
├── Medley.IdentityServer/    # Duende IdentityServer
├── SharedKernel/             # Cross-cutting abstractions
└── Modules/{ModuleName}/     # Feature modules (Domain/Application/Infrastructure/Contracts)
```

**Key rules:** Module isolation via integration events, layer dependencies (Domain→Application→Infrastructure→Presentation), database-per-schema `[ModuleName].[TableName]`, BFF pattern (no tokens in browser).

## Code Style

File-scoped namespaces, primary constructors, collection expressions, nullable reference types (warnings as errors), C# 14 `field` keyword, xUnit v3 + Shouldly, test naming: `MethodName_Scenario_ExpectedBehavior`.

## Commit Convention

```
<type>(<scope>): <description>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `build`, `ci`

## Detailed Rules

### Always-Loaded (`.claude/rules/`)

Cross-cutting rules load at startup:

- **`commands.md`** - Build, test, and migration commands
- **`architecture/`** - CQRS, layers, modules, DI patterns
- **`database/`** - EF Core patterns and conventions
- **`domain/`** - DDD patterns, validation
- **`security/`** - Secrets handling
- **`infrastructure/`** - Logging, observability

### Lazy-Loaded (Nested CLAUDE.md)

Context-specific rules load only when working in those directories:

- **`tests/CLAUDE.md`** - Testing conventions, xUnit v3, Shouldly
- **`src/Medley.Web/CLAUDE.md`** - Blazor components, REST API, BFF authentication
