# MDYCQRS001: Command not returning Result

| Property | Value |
|----------|-------|
| **Rule ID** | MDYCQRS001 |
| **Category** | Medley.CQRS |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A command returns a raw type instead of `Result<T>`.

## Rule description

Commands should return `Result<T>` for explicit error handling instead of throwing exceptions for business failures.

Using `Result<T>` makes error handling explicit and enables railway-oriented programming patterns.

## How to fix violations

Wrap the return type in `Result<T>`:

```csharp
// Bad - throwing exceptions for business failures
public record CreateUserCommand(string Email) : IRequest<UserId>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        if (await _repo.ExistsAsync(cmd.Email, ct))
            throw new DuplicateEmailException(); // Bad - exception for expected failure

        var user = User.Create(cmd.Email);
        return user.Id;
    }
}

// Good - explicit error handling
public record CreateUserCommand(string Email) : IRequest<Result<UserId>>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    public async Task<Result<UserId>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        if (await _repo.ExistsAsync(cmd.Email, ct))
            return Result.Failure<UserId>(Error.Conflict("Email already registered"));

        var user = User.Create(cmd.Email);
        return Result.Success(user.Id);
    }
}
```

## When to suppress

Suppress for:
- Integration with legacy code that expects exceptions
- Commands that delegate to external services with their own error handling

## Related rules

- [MDYRES001](MDYRES001.md) - Throwing for business failure
- [MDYRES002](MDYRES002.md) - Result value not checked
