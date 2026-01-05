# MDYRES001: Throwing for business failure

| Property | Value |
|----------|-------|
| **Rule ID** | MDYRES001 |
| **Category** | Medley.Result |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

Code throws an exception for an expected business failure instead of using `Result<T>.Failure()`.

## Rule description

Use `Result<T>.Failure()` for expected business failures, not exceptions. Exceptions should be reserved for truly exceptional circumstances (programmer errors, system failures).

Expected failures include:
- Validation errors
- Resource not found
- Business rule violations
- Conflicts (duplicate entries)

## How to fix violations

Return a failure Result instead of throwing:

```csharp
// Bad - exception for expected failure
public async Task<Order> PlaceOrderAsync(OrderId id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct);

    if (order is null)
        throw new NotFoundException($"Order {id} not found");  // Expected case

    if (order.Items.Count == 0)
        throw new ValidationException("Order must have items");  // Business rule

    // ...
}

// Good - Result pattern
public async Task<Result<Order>> PlaceOrderAsync(OrderId id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct);

    if (order is null)
        return Result.Failure<Order>(Error.NotFound($"Order {id} not found"));

    if (order.Items.Count == 0)
        return Result.Failure<Order>(Error.Validation("Order must have items"));

    return Result.Success(order);
}
```

## When to suppress

Suppress for:
- Legacy code integration
- Framework methods that expect exceptions

## Related rules

- [MDYCQRS001](MDYCQRS001.md) - Command not returning Result
- [MDYRES002](MDYRES002.md) - Result value not checked
