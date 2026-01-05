# MDYCQRS005: Validator not with command

| Property | Value |
|----------|-------|
| **Rule ID** | MDYCQRS005 |
| **Category** | Medley.CQRS |
| **Severity** | Info |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A validator is not in the same folder as its associated command.

## Rule description

Validators should be co-located with their commands for discoverability. When looking at a command, developers should immediately find its validation rules.

## How to fix violations

Move the validator to the same folder as its command:

```
// Bad - validators in separate folder
Application/
├── Features/Users/CreateUser/
│   ├── CreateUserCommand.cs
│   └── CreateUserHandler.cs
└── Validators/
    └── CreateUserValidator.cs  // Hard to discover

// Good - validator with command
Application/
└── Features/Users/CreateUser/
    ├── CreateUserCommand.cs
    ├── CreateUserHandler.cs
    └── CreateUserValidator.cs  // Easy to find
```

## When to suppress

Suppress for:
- Shared validators used by multiple commands
- Modules with different organizational patterns

## Related rules

- [MDYNAME002](MDYNAME002.md) - Validator missing suffix
- [MDYCQRS004](MDYCQRS004.md) - Handler not in Features folder
