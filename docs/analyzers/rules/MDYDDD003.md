# MDYDDD003: Domain logic in handler

| Property | Value |
|----------|-------|
| **Rule ID** | MDYDDD003 |
| **Category** | Medley.DDD |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A handler contains business logic that should be in the domain layer.

## Rule description

Complex business logic should live in the domain layer (entities, value objects, domain services), not in handlers. Handlers orchestrate use cases but delegate business rules to the domain.

The analyzer detects patterns like:
- Multiple conditional branches with business rules
- Direct manipulation of entity state
- Business calculations that could be entity methods

## How to fix violations

Move business logic to the domain:

```csharp
// Bad - business logic in handler
public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId, ct);

        // Business logic leaked into handler
        if (order.Items.Count == 0)
            return Result.Failure<OrderId>(Error.Validation("Order must have items"));

        if (order.Total < 10m)
            return Result.Failure<OrderId>(Error.Validation("Minimum order is $10"));

        order.Status = OrderStatus.Placed;
        order.PlacedAt = DateTime.UtcNow;
        // ...
    }
}

// Good - logic in domain
public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId, ct);

        var result = order.Place();  // Domain method encapsulates rules

        if (result.IsFailure)
            return Result.Failure<OrderId>(result.Error);

        return Result.Success(order.Id);
    }
}

// In Order entity
public Result Place()
{
    if (Items.Count == 0)
        return Result.Failure(Error.Validation("Order must have items"));

    if (Total < MinimumOrderValue)
        return Result.Failure(Error.Validation($"Minimum order is {MinimumOrderValue:C}"));

    Status = OrderStatus.Placed;
    PlacedAt = DateTime.UtcNow;
    AddDomainEvent(new OrderPlacedEvent(Id));

    return Result.Success();
}
```

## When to suppress

Suppress for simple CRUD operations or thin handlers.

## Related rules

- [MDYDDD002](MDYDDD002.md) - Public setter on entity
