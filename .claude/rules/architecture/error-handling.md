---
paths:
  - "**/*.cs"
---

<!-- ~200 tokens -->

# Error Handling

Use Result<T> for expected failures, exceptions for unexpected failures.

See [docs/architecture/shared-kernel.md](../../../docs/architecture/shared-kernel.md) for complete implementation patterns.

## When to Throw vs Return Result<T>

| Scenario | Approach |
|----------|----------|
| Expected business failure | `Result<T>.Failure` |
| Programmer error | Throw exception |
| External system failure | Throw, wrap at boundary |
| Resource not found | `Result<T>.Failure` |

## Quick Reference

```csharp
// Railway-oriented programming
result.Bind(x => Validate(x))    // Chain Result<T>-returning operations
result.Map(x => transform(x))    // Transform success value
result.Match(success, failure)   // Pattern match

// API mapping
result.Match(
    success => TypedResults.Ok(success),
    error => error switch
    {
        NotFoundError => TypedResults.NotFound(ToProblemDetails(error)),
        ValidationError => TypedResults.BadRequest(ToProblemDetails(error)),
        _ => TypedResults.Problem(ToProblemDetails(error))
    }
);
```

## Prohibited

- Catching `Exception` without re-throwing (except at boundary)
- Empty catch blocks
- Using exceptions for control flow
- Returning null instead of Result<T> for failures
- Logging sensitive data in error messages
- Exposing stack traces to API clients
