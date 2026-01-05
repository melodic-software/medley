# MDYASYNC002: CancellationToken not passed

| Property | Value |
|----------|-------|
| **Rule ID** | MDYASYNC002 |
| **Category** | Medley.Async |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A method has a `CancellationToken` parameter but doesn't pass it to downstream async calls.

## Rule description

Pass `CancellationToken` to all downstream async calls to propagate cancellation through the call chain. This ensures the entire operation can be cancelled, not just the top-level call.

## How to fix violations

Pass the token to all async calls:

```csharp
// Bad - token not passed
public async Task<OrderDto> ProcessOrderAsync(OrderId id, CancellationToken ct)
{
    var order = await _orderRepo.GetByIdAsync(id);  // Token not passed!
    var customer = await _customerRepo.GetByIdAsync(order.CustomerId);  // Token not passed!

    await _notificationService.SendAsync(customer.Email, "Order processed");  // Token not passed!

    return order.ToDto();
}

// Good - token passed to all calls
public async Task<OrderDto> ProcessOrderAsync(OrderId id, CancellationToken ct)
{
    var order = await _orderRepo.GetByIdAsync(id, ct);
    var customer = await _customerRepo.GetByIdAsync(order.CustomerId, ct);

    await _notificationService.SendAsync(customer.Email, "Order processed", ct);

    return order.ToDto();
}
```

## When to suppress

Suppress for:
- Calls to APIs that don't accept `CancellationToken`
- Operations that must complete even if cancellation is requested

## Related rules

- [MDYASYNC001](MDYASYNC001.md) - Missing CancellationToken
