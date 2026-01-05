# MDYNAME004: Specification missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME004 |
| **Category** | Medley.Naming |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class extends `Specification<T>` but doesn't end with `Specification`.

## Rule description

Specification pattern classes should end with `Specification` suffix to clearly indicate their purpose in query composition.

## How to fix violations

Rename the class to end with `Specification`:

```csharp
// Bad
public class ActiveUsers : Specification<User> { }
public class OrdersByCustomer : Specification<Order> { }

// Good
public class ActiveUsersSpecification : Specification<User> { }
public class OrdersByCustomerSpecification : Specification<Order> { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Specification` suffix.

## When to suppress

Suppress for custom specification implementations with different naming conventions.

## Related rules

- [MDYNAME001](MDYNAME001.md) - Repository missing suffix
