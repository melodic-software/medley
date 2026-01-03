# Medley

A modular monolith application built with modern .NET technologies.

## Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 (LTS) | Runtime & SDK |
| C# | 14 | Language |
| Blazor | Interactive Auto | UI Framework |
| Aspire | 13.x | Orchestration & Observability |
| Duende IdentityServer | 7.4.x | Security Token Service |
| Duende BFF | Latest | Backend for Frontend (Blazor) |

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

## Project Structure (Planned)

```
medley/
├── src/
│   ├── Medley.AppHost/           # Aspire orchestration
│   ├── Medley.ServiceDefaults/   # Shared Aspire defaults
│   ├── Medley.Web/               # Blazor UI + BFF
│   ├── Medley.IdentityServer/    # Duende IdentityServer
│   ├── Medley.Analyzers/         # Custom Roslyn analyzers
│   └── Modules/
│       └── [Feature]/
│           ├── Domain/
│           ├── Application/
│           ├── Infrastructure/
│           └── Presentation/
├── tests/
│   ├── Architecture.Tests/       # NetArchTest rules
│   └── [Feature].Tests/
└── docs/
```

## Getting Started

> Documentation coming soon.

## License

Private - All rights reserved.
