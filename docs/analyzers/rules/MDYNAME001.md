# MDYNAME001: Repository missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME001 |
| **Category** | Medley.Naming |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class implements `IRepository<T>` or a repository interface but doesn't end with `Repository`.

## Rule description

Repository implementations should end with `Repository` suffix for consistency and discoverability.

## How to fix violations

Rename the class to end with `Repository`:

```csharp
// Bad
public class UserStore : IUserRepository { }
public class OrderData : IRepository<Order> { }

// Good
public class UserRepository : IUserRepository { }
public class OrderRepository : IRepository<Order> { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Repository` suffix.

## When to suppress

Suppress if you have a valid naming convention that differs from this pattern.

```csharp
#pragma warning disable MDYNAME001
public class UserStore : IUserRepository { }
#pragma warning restore MDYNAME001
```

## Related rules

- [MDYNAME006](MDYNAME006.md) - Configuration missing suffix
