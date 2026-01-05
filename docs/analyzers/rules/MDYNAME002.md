# MDYNAME002: Validator missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME002 |
| **Category** | Medley.Naming |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class extends `AbstractValidator<T>` but doesn't end with `Validator`.

## Rule description

FluentValidation validators should end with `Validator` suffix for consistency and to clearly indicate their purpose.

## How to fix violations

Rename the class to end with `Validator`:

```csharp
// Bad
public class CreateUserRules : AbstractValidator<CreateUserCommand> { }
public class OrderValidation : AbstractValidator<CreateOrderCommand> { }

// Good
public class CreateUserValidator : AbstractValidator<CreateUserCommand> { }
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand> { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Validator` suffix.

## When to suppress

Suppress for composite validators or specialized validation classes.

## Related rules

- [MDYNAME003](MDYNAME003.md) - Handler missing suffix
- [MDYCQRS005](MDYCQRS005.md) - Validator not with command
