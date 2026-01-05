# MDYNAME005: Service missing suffix

| Property | Value |
|----------|-------|
| **Rule ID** | MDYNAME005 |
| **Category** | Medley.Naming |
| **Severity** | Info |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

A class appears to be a Domain or Application service but doesn't end with `Service`.

## Rule description

Domain and Application services should end with `Service` suffix for clarity. This rule uses heuristics to detect service-like classes (e.g., classes with names like `*Manager`, `*Processor`, `*Coordinator` that act as services).

## How to fix violations

Rename the class to end with `Service`:

```csharp
// Bad
public class EmailSender { }
public class PaymentProcessor { }
public class NotificationManager { }

// Good
public class EmailService { }
public class PaymentService { }
public class NotificationService { }
```

## Code fix

A code fix is available that automatically renames the class to add the `Service` suffix.

## When to suppress

This is an informational rule. Suppress for:
- Classes that are not services despite similar naming
- Framework-specific classes with established naming patterns

## Related rules

- [MDYNAME001](MDYNAME001.md) - Repository missing suffix
