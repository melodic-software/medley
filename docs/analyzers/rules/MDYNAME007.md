# MDYNAME007: DTO missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME007 |
| **Category** | Medley.Naming |
| **Severity** | Info |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class appears to be a Data Transfer Object but doesn't end with `Dto`.

## Rule description

Data Transfer Objects should end with `Dto` suffix to distinguish them from domain entities and value objects.

This rule uses heuristics to detect DTO-like classes (e.g., records in Contracts projects, response/request objects).

## How to fix violations

Rename the class to end with `Dto`:

```csharp
// Bad
public record UserResponse(Guid Id, string Email, string Name);
public class OrderDetails { }
public record CustomerData(string Name, string Email);

// Good
public record UserDto(Guid Id, string Email, string Name);
public class OrderDto { }
public record CustomerDto(string Name, string Email);
```

## Code fix

A code fix is available that automatically renames the class to add the `Dto` suffix.

## When to suppress

This is an informational rule. Suppress for:
- API-specific request/response models that use different conventions
- View models in Blazor/MVC projects
- Records that represent domain concepts, not DTOs

## Related rules

- [MDYNAME005](MDYNAME005.md) - Service missing suffix
