# MDYNAME006: Configuration missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME006 |
| **Category** | Medley.Naming |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class implements `IEntityTypeConfiguration<T>` but doesn't end with `Configuration`.

## Rule description

EF Core entity configurations should end with `Configuration` suffix for consistency and discoverability.

## How to fix violations

Rename the class to end with `Configuration`:

```csharp
// Bad
public class UserMapping : IEntityTypeConfiguration<User> { }
public class OrderEntityConfig : IEntityTypeConfiguration<Order> { }

// Good
public class UserConfiguration : IEntityTypeConfiguration<User> { }
public class OrderConfiguration : IEntityTypeConfiguration<Order> { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Configuration` suffix.

## When to suppress

Suppress for custom EF Core configuration patterns.

## Related rules

- [MDYNAME001](MDYNAME001.md) - Repository missing suffix
