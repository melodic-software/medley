# Project Structure

Complete project structure for the Medley modular monolith.

## Solution Layout

```
Medley.slnx

src/
├── Medley.AppHost/              # Aspire orchestrator
├── Medley.ServiceDefaults/      # Aspire shared defaults
├── Medley.Web/                  # Blazor UI + BFF (modular monolith host)
├── Medley.IdentityServer/       # Duende IdentityServer
│
├── SharedKernel/                # Cross-cutting DDD building blocks
│   ├── SharedKernel/            # Core: Entity, ValueObject, Result, DomainEvent (planned)
│   ├── SharedKernel.Application/   # CQRS: Commands, Queries, Behaviors (planned)
│   ├── SharedKernel.Infrastructure/  # EF interceptors, repositories (planned)
│   ├── SharedKernel.Contracts/  # Integration events, shared DTOs (planned)
│   ├── SharedKernel.Analyzers/  # Custom Roslyn analyzers (naming, architecture)
│   └── SharedKernel.Analyzers.CodeFixes/  # Code fix providers for analyzers
│
└── Modules/
    └── {ModuleName}/            # e.g., Users, Orders
        ├── {ModuleName}.Domain/
        ├── {ModuleName}.Application/
        ├── {ModuleName}.Infrastructure/
        └── {ModuleName}.Contracts/

tests/
├── SharedKernel/
│   ├── SharedKernel.Analyzers.Tests/  # Analyzer unit tests
│   ├── SharedKernel.Tests/            # (planned)
│   ├── SharedKernel.Application.Tests/  # (planned)
│   └── SharedKernel.Infrastructure.Tests/  # (planned)
├── Modules/
│   └── {ModuleName}/
│       ├── {ModuleName}.Domain.Tests/
│       ├── {ModuleName}.Application.Tests/
│       └── {ModuleName}.Integration.Tests/
└── Architecture.Tests/          # NetArchTest boundary enforcement (planned)

docs/
infrastructure/
```

## Project Naming Convention

| Project | PackageId (if published) |
|---------|--------------------------|
| `Medley.AppHost` | N/A (not packaged) |
| `Medley.ServiceDefaults` | N/A |
| `Medley.Web` | N/A |
| `Medley.IdentityServer` | N/A |
| `SharedKernel` | `Melodic.SharedKernel` |
| `SharedKernel.Application` | `Melodic.SharedKernel.Application` |
| `SharedKernel.Infrastructure` | `Melodic.SharedKernel.Infrastructure` |
| `SharedKernel.Contracts` | `Melodic.SharedKernel.Contracts` |
| `SharedKernel.Analyzers` | `Melodic.SharedKernel.Analyzers` |
| `SharedKernel.Analyzers.CodeFixes` | `Melodic.SharedKernel.Analyzers.CodeFixes` |
| `{Module}.Domain` | N/A (internal) |
| `{Module}.Application` | N/A (internal) |
| `{Module}.Infrastructure` | N/A (internal) |
| `{Module}.Contracts` | N/A (internal) |

**Principle**: Company prefix (`Melodic.`) only in PackageId, not namespaces.

## Layer Dependencies

```
Domain → Application → Infrastructure → Presentation
```

- **Domain** - No dependencies on other layers (only .NET BCL)
- **Application** - Depends only on Domain (+ MediatR for CQRS)
- **Infrastructure** - Implements abstractions from inner layers
- **Presentation** - Entry point, depends on all layers for DI

## File Naming for Generics

Separate files using curly braces:

```
Result.cs                    # Non-generic
Result{T}.cs                 # Generic
ICommand.cs                  # Non-generic marker
ICommand{TResult}.cs         # Generic with result
Entity{TId}.cs               # Entity with typed ID
```

## Related Documentation

- [SharedKernel Patterns](shared-kernel.md)
- [Module Patterns](module-patterns.md)
- [CQRS Patterns](cqrs-patterns.md)
