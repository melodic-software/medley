# MDYASYNC001: Missing CancellationToken

| Property | Value |
|----------|-------|
| **Rule ID** | MDYASYNC001 |
| **Category** | Medley.Async |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

An async method doesn't accept a `CancellationToken` parameter.

## Rule description

Async methods should accept `CancellationToken` to support cancellation. This allows callers to cancel long-running operations and enables graceful shutdown.

## How to fix violations

Add a `CancellationToken` parameter:

```csharp
// Bad - no cancellation support
public async Task<User> GetUserAsync(Guid userId)
{
    return await _context.Users.FindAsync(userId);
}

// Good - accepts CancellationToken
public async Task<User> GetUserAsync(Guid userId, CancellationToken ct = default)
{
    return await _context.Users.FindAsync(userId, ct);
}

// For public APIs, use default parameter
public async Task<OrderDto> GetOrderAsync(OrderId id, CancellationToken ct = default)

// For internal methods, require the token
internal async Task<Order> GetOrderInternalAsync(OrderId id, CancellationToken ct)
```

## Code fix

A code fix is available that adds `CancellationToken ct = default` as the last parameter.

## When to suppress

Suppress for:
- Event handlers that don't support cancellation
- Short-running operations where cancellation isn't needed

## Related rules

- [MDYASYNC002](MDYASYNC002.md) - CancellationToken not passed
- [MDYASYNC003](MDYASYNC003.md) - Async void method
