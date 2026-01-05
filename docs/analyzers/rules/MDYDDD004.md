# MDYDDD004: Aggregate references by object

| Property | Value |
|----------|-------|
| **Rule ID** | MDYDDD004 |
| **Category** | Medley.DDD |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

An aggregate holds a direct reference to another aggregate instead of referencing by ID.

## Rule description

Aggregates should reference other aggregates by ID, not by object reference. This maintains aggregate boundaries and prevents large object graphs that complicate persistence and transactional boundaries.

## How to fix violations

Change object references to ID references:

```csharp
// Bad - direct object reference
public class Order : AggregateRoot<OrderId>
{
    public Customer Customer { get; private set; }  // Object reference
    public Product Product { get; private set; }    // Object reference
}

// Good - ID references
public class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }  // ID reference
    public ProductId ProductId { get; private set; }    // ID reference

    // Load related aggregates when needed through services
}
```

When you need the related aggregate's data:

```csharp
public class GetOrderWithCustomerHandler
{
    public async Task<OrderWithCustomerDto> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(query.OrderId, ct);
        var customer = await _customerRepo.GetByIdAsync(order.CustomerId, ct);

        return new OrderWithCustomerDto(order, customer);
    }
}
```

## When to suppress

Suppress for:
- Entities within the same aggregate (not aggregates themselves)
- Read models designed for specific queries

## Related rules

- [MDYDDD005](MDYDDD005.md) - Value object with identity
