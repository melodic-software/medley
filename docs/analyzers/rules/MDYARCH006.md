# MDYARCH006: Contracts project references internal project

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH006 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

A Contracts project references an internal module project (Domain, Application, or Infrastructure).

## Rule description

Contracts projects expose the public API of a module for cross-module communication. They must contain only:
- Integration events
- Public DTOs
- Interface definitions for module APIs
- Constants and enums needed by consumers

Contracts must not reference internal implementation details to maintain loose coupling between modules.

## How to fix violations

Move shared types to Contracts instead of referencing internal projects:

```xml
<!-- Bad - Contracts referencing internal project -->
<ProjectReference Include="..\Orders.Domain\Orders.Domain.csproj" />

<!-- Good - Contracts is self-contained -->
<!-- No internal project references -->
```

```csharp
// In Contracts - define only what consumers need
public record OrderCreatedEvent(Guid OrderId, DateTime CreatedAt);

public record OrderDto(Guid Id, string Status, decimal Total);

public interface IOrderQueries
{
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct);
}
```

## When to suppress

This rule should never be suppressed. Contracts isolation is fundamental to module independence.

## Related rules

- [MDYARCH001](MDYARCH001.md) - Cross-module direct reference
