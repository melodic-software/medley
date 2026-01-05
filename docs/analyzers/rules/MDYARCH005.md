# MDYARCH005: Application layer references Infrastructure layer

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH005 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

The Application layer contains a reference to the Infrastructure layer.

## Rule description

Clean Architecture requires the Application layer to depend only on the Domain layer, not on Infrastructure. Application layer orchestrates use cases using abstractions defined in Domain.

Infrastructure implementations are injected at runtime through dependency injection configured in the Presentation layer.

## How to fix violations

Remove Infrastructure dependencies from Application:

```csharp
// Bad - Application referencing Infrastructure
using MyModule.Infrastructure.Persistence;

public class CreateOrderHandler
{
    private readonly AppDbContext _context; // Infrastructure type
}

// Good - Application uses Domain abstractions
using MyModule.Domain.Repositories;

public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
}
```

## When to suppress

This rule should not be suppressed. Use the opt-out mechanisms for vertical slice modules.

## Related rules

- [MDYARCH003](MDYARCH003.md) - Domain references Application
- [MDYARCH004](MDYARCH004.md) - Domain references Infrastructure
