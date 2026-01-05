# MDYARCH004: Domain layer references Infrastructure layer

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH004 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

The Domain layer contains a reference to the Infrastructure layer.

## Rule description

Clean Architecture requires the Domain layer to have no dependencies on outer layers. Infrastructure concerns like database access, external APIs, and file I/O must not leak into the Domain.

The Domain layer defines abstractions (interfaces) that Infrastructure implements.

## How to fix violations

Remove Infrastructure dependencies from Domain:

```csharp
// Bad - Domain referencing Infrastructure
using MyModule.Infrastructure.Persistence;

public class OrderService
{
    private readonly AppDbContext _context; // Infrastructure type
}

// Good - Domain uses abstractions
public class OrderService
{
    private readonly IOrderRepository _repository; // Interface defined in Domain
}
```

Define repository interfaces in Domain, implement in Infrastructure:

```csharp
// Domain/IOrderRepository.cs
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct);
}

// Infrastructure/Repositories/OrderRepository.cs
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    // Implementation
}
```

## When to suppress

This rule should not be suppressed. Use the opt-out mechanisms for vertical slice modules.

## Related rules

- [MDYARCH003](MDYARCH003.md) - Domain references Application
- [MDYARCH005](MDYARCH005.md) - Application references Infrastructure
