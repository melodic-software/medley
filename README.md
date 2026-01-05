# Medley

[![CI](https://github.com/melodic-software/medley/actions/workflows/ci.yml/badge.svg)](https://github.com/melodic-software/medley/actions/workflows/ci.yml)

A modular monolith application built with modern .NET technologies.

> **Status**: Pre-MVP scaffolding phase. Build infrastructure, CI/CD, and architectural guidelines are in place. Source code implementation is next.

## Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 (LTS) | Runtime & SDK |
| C# | 14 | Language |
| Blazor | Interactive Auto | UI Framework |
| [Aspire](https://aspire.dev) | 13.1+ | Orchestration & Observability |
| Duende IdentityServer | 7.4.x | Security Token Service |
| Duende BFF | Latest | Backend for Frontend (Blazor) |

> **Historical Note** (November 2025): Aspire version numbering jumped from 9.x to 13.x in November 2025 to align with its rebranding as a polyglot platform. See the [Aspire 13 announcement](https://devblogs.microsoft.com/aspire/aspire13/) for historical context.

## Architecture

### Modular Monolith

This application follows a **modular monolith** architecture - combining the simplicity of a monolith deployment with the modularity of microservices.

### Clean Architecture

Each module follows **Clean Architecture** principles with clear separation:

- **Domain** - Entities, value objects, domain events
- **Application** - Use cases, commands, queries (CQS/CQRS)
- **Infrastructure** - External concerns, persistence, integrations
- **Presentation** - API endpoints, Blazor UI

### Vertical Slices

Features are organized as **vertical slices** across layers, keeping related code together.

### CQRS Pattern

Commands and Queries are separated following the **CQS/CQRS** pattern for clarity and scalability.

### Gateway Architecture (Planned)

External traffic routes through dual YARP gateways:
- **App Gateway** (`apps.melodicsoftware.com`) - UI, BFF, IdentityServer
- **API Gateway** (`api.melodicsoftware.com`) - Module APIs

See [BACKLOG.md](docs/BACKLOG.md#gateway-architecture-yarp-via-aspire) for implementation details.

## Code Quality

### Compile-Time Enforcement

- **Custom Roslyn Analyzers** - Enforce architectural rules at compile time
- **Built-in .NET Analyzers** - Code quality and security rules

### Test-Time Enforcement

- **Architecture Tests** - NetArchTest.Rules for structural validation
- **Boundary Enforcement** - Verify layer dependencies and module isolation

### When to Use Each

| Tool | Timing | Best For |
|------|--------|----------|
| Roslyn Analyzers | Compile-time | Style, naming, API usage, immediate feedback |
| Architecture Tests | Post-build | Layer dependencies, module boundaries, structural rules |

## Security

- **Duende IdentityServer 7.4** - OAuth 2.0 / OpenID Connect provider
- **Duende BFF** - Secure token handling for Blazor UI
- **Passkey Support** - .NET 10 native passkey integration

## Project Structure

> **Planned** - This is the target structure to be implemented.

```
medley/
├── src/
│   ├── Medley.AppHost/              # Aspire orchestrator
│   ├── Medley.ServiceDefaults/      # Aspire shared defaults
│   ├── Medley.Web/                  # Blazor UI + BFF (modular monolith host)
│   ├── Medley.IdentityServer/       # Duende IdentityServer
│   │
│   ├── SharedKernel/                # Cross-cutting DDD building blocks
│   │   ├── SharedKernel/            # Core: Entity, ValueObject, Result, DomainEvent
│   │   ├── SharedKernel.Application/   # CQRS: Commands, Queries, Behaviors
│   │   ├── SharedKernel.Infrastructure/  # EF interceptors, repositories
│   │   ├── SharedKernel.Contracts/  # Integration events, shared DTOs
│   │   └── SharedKernel.Analyzers/  # Custom Roslyn analyzers (naming, architecture)
│   │
│   └── Modules/
│       └── {ModuleName}/            # e.g., Users, Orders
│           ├── {ModuleName}.Domain/
│           ├── {ModuleName}.Application/
│           ├── {ModuleName}.Infrastructure/
│           └── {ModuleName}.Contracts/
│
├── tests/
│   ├── SharedKernel/
│   │   ├── SharedKernel.Tests/
│   │   ├── SharedKernel.Application.Tests/
│   │   └── SharedKernel.Infrastructure.Tests/
│   ├── Modules/
│   │   └── {ModuleName}/
│   │       ├── {ModuleName}.Domain.Tests/
│   │       ├── {ModuleName}.Application.Tests/
│   │       └── {ModuleName}.Integration.Tests/
│   └── Architecture.Tests/          # NetArchTest boundary enforcement
│
├── docs/
└── infrastructure/                  # IaC (Bicep/Terraform)
```

See [docs/architecture/](docs/architecture/) for detailed structure documentation.

### Solution Files

| Solution | Purpose |
|----------|---------|
| `Medley.slnx` | **Main solution** - Full application with all modules, tests, and infrastructure |
| `Medley.Analyzers.slnx` | **Focused solution** - Roslyn analyzers only, for faster IDE load during analyzer development |

> **Tip**: Use `Medley.Analyzers.slnx` when developing custom analyzers to avoid loading the full solution.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Development

Source code scaffolding is in progress. See [CONTRIBUTING.md](CONTRIBUTING.md) for development workflow and conventions.

## License

Proprietary - see [LICENSE](LICENSE) for details.

Copyright 2026 Melodic Software. All rights reserved.
