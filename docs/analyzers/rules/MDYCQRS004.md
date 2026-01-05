# MDYCQRS004: Handler not in Features folder

| Property | Value |
|----------|-------|
| **Rule ID** | MDYCQRS004 |
| **Category** | Medley.CQRS |
| **Severity** | Info |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A handler is not located in a `Features/{FeatureName}/` folder structure.

## Rule description

Vertical slice organization groups related code by feature. Handlers should be organized in `Features/{FeatureName}/` folders alongside their commands, queries, and validators.

## How to fix violations

Move the handler to the appropriate feature folder:

```
// Bad - handlers scattered
Application/
├── Handlers/
│   ├── CreateUserHandler.cs
│   └── GetUserHandler.cs
├── Commands/
│   └── CreateUserCommand.cs
└── Queries/
    └── GetUserQuery.cs

// Good - vertical slice organization
Application/
└── Features/
    └── Users/
        ├── CreateUser/
        │   ├── CreateUserCommand.cs
        │   ├── CreateUserHandler.cs
        │   └── CreateUserValidator.cs
        └── GetUser/
            ├── GetUserQuery.cs
            └── GetUserHandler.cs
```

## When to suppress

Suppress for:
- Modules using traditional layered organization
- Simple modules where vertical slices add unnecessary complexity

Use `[assembly: ModuleArchitecture(ModuleStyle.CleanArchitecture)]` to indicate non-vertical slice organization.

## Related rules

- [MDYCQRS005](MDYCQRS005.md) - Validator not with command
