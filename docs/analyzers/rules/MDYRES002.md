# MDYRES002: Result value not checked

| Property | Value |
|----------|-------|
| **Rule ID** | MDYRES002 |
| **Category** | Medley.Result |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

Code accesses `Result.Value` without first checking `IsSuccess`.

## Rule description

Always check `IsSuccess` before accessing `Result.Value` to avoid runtime errors. Accessing `.Value` on a failed Result may throw or return invalid data.

## How to fix violations

Check `IsSuccess` before accessing the value:

```csharp
// Bad - accessing Value without checking
var result = await _service.GetUserAsync(userId, ct);
var user = result.Value;  // May throw if result is failure

// Good - check first
var result = await _service.GetUserAsync(userId, ct);
if (result.IsFailure)
{
    return Result.Failure<OrderDto>(result.Error);
}
var user = result.Value;  // Safe to access

// Better - use Match or Map
var result = await _service.GetUserAsync(userId, ct);
return result.Match(
    success: user => ProcessUser(user),
    failure: error => HandleError(error)
);

// Best - use Bind for chaining
var result = await _service.GetUserAsync(userId, ct)
    .Bind(user => ValidateUser(user))
    .Map(user => CreateOrder(user));
```

## When to suppress

Suppress if you have external guarantees that the Result is successful.

## Related rules

- [MDYRES001](MDYRES001.md) - Throwing for business failure
- [MDYRES003](MDYRES003.md) - Result not awaited
