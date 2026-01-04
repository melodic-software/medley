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

## Commands Reference

### EF Core Migrations

Per-module migrations - each module owns its schema:

```bash
# Create migration (example: Auth module)
dotnet ef migrations add MigrationName \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web

# Generate SQL script for production
dotnet ef migrations script --idempotent \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web \
    --output migration.sql

# Apply migration (development only)
dotnet ef database update \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web
```

### Common Operations

| Task | Command |
|------|---------|
| Clean build | `dotnet clean && dotnet build` |
| Watch mode | `dotnet watch --project src/Medley.Web` |
| Run tests with coverage | `dotnet test --collect:"XPlat Code Coverage"` |
| List outdated packages | `dotnet outdated` |
| Update packages | `dotnet outdated --upgrade` |

**Prohibited:** Running `dotnet ef database update` in production (use migration scripts), committing without running tests first, using `--force` flags without explicit justification, running migrations across module boundaries.

---

## Detailed Rules

### Always-Loaded (`.claude/rules/`)

Cross-cutting rules load at startup (~4,600 tokens):

- **`code-style/`** - C# conventions, comments, constants/magic strings
- **`architecture/`** - Layer dependencies, DI patterns, error handling
- **`async/`** - Task handling and cancellation tokens
- **`security/`** - Secrets handling

### Lazy-Loaded (Nested CLAUDE.md)

Context-specific rules load only when working in those directories:

- **`.claude/CLAUDE.md`** - Rules authoring guide (when maintaining rules)
- **`src/CLAUDE.md`** - Infrastructure patterns (logging, observability, EF Core)
- **`src/SharedKernel/CLAUDE.md`** - Application layer patterns (CQRS, validation)
- **`src/Modules/CLAUDE.md`** - Module development (boundaries, DDD patterns)
- **`src/Medley.Web/CLAUDE.md`** - Blazor components, REST API, BFF authentication
- **`tests/CLAUDE.md`** - Testing conventions, xUnit v3, Shouldly
- **`.github/CLAUDE.md`** - GitHub workflows and security
