# MDYARCH001: Cross-module direct reference detected

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH001 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

A module directly references another module's internal project (Domain, Application, or Infrastructure) instead of using the Contracts project.

## Rule description

Modules in a modular monolith must maintain isolation. Direct references between module internals violate this boundary and create tight coupling that undermines the modular architecture.

Modules should only communicate through their Contracts projects, which expose:
- Integration events
- Shared DTOs
- Public interfaces

## How to fix violations

Replace the direct reference with a reference to the target module's Contracts project:

```csharp
// Bad - direct reference to internal project
using Orders.Domain.Entities;
using Orders.Application.Services;

// Good - reference through Contracts
using Orders.Contracts.Events;
using Orders.Contracts.Dtos;
```

Update your `.csproj` to reference only Contracts projects:

```xml
<!-- Bad -->
<ProjectReference Include="..\Orders\Orders.Domain\Orders.Domain.csproj" />

<!-- Good -->
<ProjectReference Include="..\Orders\Orders.Contracts\Orders.Contracts.csproj" />
```

## When to suppress

This rule should rarely be suppressed. If you have a valid architectural reason, document it clearly.

## Related rules

- [MDYARCH002](MDYARCH002.md) - Cross-module database access
- [MDYARCH006](MDYARCH006.md) - Contracts references internal project
