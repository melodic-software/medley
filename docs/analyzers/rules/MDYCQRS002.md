# MDYCQRS002: Query has side effects

| Property | Value |
|----------|-------|
| **Rule ID** | MDYCQRS002 |
| **Category** | Medley.CQRS |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A query handler modifies state (calls Add, Update, Delete, SaveChanges, etc.).

## Rule description

CQRS separates reads (Queries) from writes (Commands). Query handlers must be read-only and have no side effects.

State modifications belong in command handlers only.

## How to fix violations

Move state modifications to a command handler:

```csharp
// Bad - query with side effects
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(query.UserId, ct);

        // Side effect in query - VIOLATION
        user.LastAccessedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user, ct);

        return user?.ToDto();
    }
}

// Good - pure query, no side effects
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(query.UserId, ct);
        return user?.ToDto();
    }
}

// Separate command for tracking access
public record TrackUserAccessCommand(Guid UserId) : IRequest<Result<Unit>>;
```

## When to suppress

This rule should not be suppressed. If you need side effects, use a command.

## Related rules

- [MDYCQRS001](MDYCQRS001.md) - Command not returning Result
- [MDYCQRS003](MDYCQRS003.md) - Multiple handlers for request
