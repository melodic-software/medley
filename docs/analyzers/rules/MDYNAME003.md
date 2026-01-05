# MDYNAME003: Handler missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME003 |
| **Category** | Medley.Naming |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class implements `IRequestHandler<TRequest, TResponse>` but doesn't end with `Handler`.

## Rule description

MediatR handlers should end with `Handler` suffix. The namespace provides context about whether it's a command or query handler.

Note: Use simple `Handler` suffix, not `CommandHandler` or `QueryHandler`. Namespace organization (`Commands/` or `Queries/`) provides that context.

## How to fix violations

Rename the class to end with `Handler`:

```csharp
// Bad
public class CreateUser : IRequestHandler<CreateUserCommand, Result<UserId>> { }
public class GetOrderProcessor : IRequestHandler<GetOrderQuery, OrderDto?> { }

// Good
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserId>> { }
public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto?> { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Handler` suffix.

## When to suppress

Suppress for specialized handler implementations that follow a different naming pattern.

## Related rules

- [MDYNAME002](MDYNAME002.md) - Validator missing suffix
- [MDYCQRS004](MDYCQRS004.md) - Handler not in Features folder
