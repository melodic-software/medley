# MDYCQRS003: Multiple handlers for request

| Property | Value |
|----------|-------|
| **Rule ID** | MDYCQRS003 |
| **Category** | Medley.CQRS |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

Multiple handler classes are registered for the same command or query.

## Rule description

Each command or query should have exactly one handler. Multiple handlers for the same request indicates a design problem or accidental duplication.

MediatR will throw at runtime if multiple handlers are registered for a single request type.

## How to fix violations

Remove duplicate handlers and consolidate logic:

```csharp
// Bad - multiple handlers for same request
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    // Implementation A
}

public class CreateUserHandlerV2 : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    // Implementation B - DUPLICATE
}

// Good - single handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    // Consolidated implementation
}
```

If you need different behavior in different contexts, consider:
- Decorators for cross-cutting concerns
- Strategy pattern injected into the handler
- Separate command types for different use cases

## When to suppress

This rule should not be suppressed.

## Related rules

- [MDYCQRS002](MDYCQRS002.md) - Query has side effects
