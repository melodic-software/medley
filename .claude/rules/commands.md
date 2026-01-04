---
paths:
  - "*.sln"
  - "*.slnx"
  - "*.csproj"
  - "**/Migrations/**"
---

<!-- ~150 tokens -->

# Commands Reference

## Build Commands

```bash
# Restore, build, test (standard workflow)
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --verbosity normal

# Run single test (xUnit filter syntax)
dotnet test --filter "FullyQualifiedName~CreateUserCommandHandlerTests"
dotnet test --filter "MethodName_Scenario_ExpectedBehavior"

# Run Aspire orchestrator (development)
dotnet run --project src/Medley.AppHost
```

## EF Core Migrations

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

## Common Operations

| Task | Command |
|------|---------|
| Clean build | `dotnet clean && dotnet build` |
| Watch mode | `dotnet watch --project src/Medley.Web` |
| Run tests with coverage | `dotnet test --collect:"XPlat Code Coverage"` |
| List outdated packages | `dotnet outdated` |
| Update packages | `dotnet outdated --upgrade` |

## Prohibited

- Running `dotnet ef database update` in production (use migration scripts)
- Committing without running tests first
- Using `--force` flags without explicit justification
- Running migrations across module boundaries
