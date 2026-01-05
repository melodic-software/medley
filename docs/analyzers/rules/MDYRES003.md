# MDYRES003: Async Result not awaited

| Property | Value |
|----------|-------|
| **Rule ID** | MDYRES003 |
| **Category** | Medley.Result |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

An async operation returning `Task<Result<T>>` is not awaited.

## Rule description

Async Result operations must be awaited. Not awaiting can lead to:
- Fire-and-forget behavior (errors silently ignored)
- Race conditions
- Resource leaks

## How to fix violations

Await the async Result operation:

```csharp
// Bad - not awaited
public async Task ProcessAsync(CancellationToken ct)
{
    _service.CreateUserAsync(command, ct);  // Not awaited!
    // Continues without waiting for result
}

// Bad - assigned but not awaited
public async Task ProcessAsync(CancellationToken ct)
{
    var task = _service.CreateUserAsync(command, ct);  // Task not awaited
    // Result never checked
}

// Good - properly awaited
public async Task<Result<Unit>> ProcessAsync(CancellationToken ct)
{
    var result = await _service.CreateUserAsync(command, ct);

    if (result.IsFailure)
        return Result.Failure<Unit>(result.Error);

    return Result.Success(Unit.Value);
}
```

## When to suppress

Suppress only for intentional fire-and-forget with proper error handling:

```csharp
// Intentional fire-and-forget with error handling
_ = Task.Run(async () =>
{
    try
    {
        await _service.SendNotificationAsync(userId, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Notification failed");
    }
});
```

## Related rules

- [MDYASYNC003](MDYASYNC003.md) - Async void method
- [MDYRES002](MDYRES002.md) - Result value not checked
