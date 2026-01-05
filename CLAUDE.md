<!-- Last reviewed: 2026-01-04 -->
<!-- ~150 tokens (hub file - references docs/ for details) -->

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

See [Project Structure](docs/architecture/project-structure.md) for complete architecture layout and layer dependencies.

## Code Style

File-scoped namespaces, primary constructors, collection expressions, nullable reference types (warnings as errors), C# 14 `field` keyword. See [tests/CLAUDE.md](tests/CLAUDE.md) for testing conventions.

## Commands

See [Database Migrations](docs/database-migrations.md) for per-module EF Core migration patterns.

| Task | Command |
|------|---------|
| Clean build | `dotnet clean && dotnet build` |
| Watch mode | `dotnet watch --project src/Medley.Web` |
| Run tests with coverage | `dotnet test --collect:"XPlat Code Coverage"` |

## Commit Convention

Format: `<type>(<scope>): <description>` - See [CONTRIBUTING.md](CONTRIBUTING.md#commit-messages-conventional-commits) for types and examples.

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
- **`src/SharedKernel/CLAUDE.md`** - DDD building blocks, Result pattern, CQRS abstractions
- **`src/Modules/CLAUDE.md`** - Module development (boundaries, vertical slices, Contracts)
- **`src/Medley.Web/CLAUDE.md`** - Blazor components, REST API, BFF authentication
- **`tests/CLAUDE.md`** - Testing conventions, xUnit v3, Shouldly
- **`.github/CLAUDE.md`** - GitHub workflows and security
