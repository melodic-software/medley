# MDYDDD002: Public setter on entity

| Property | Value |
|----------|-------|
| **Rule ID** | MDYDDD002 |
| **Category** | Medley.DDD |
| **Severity** | Warning |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

An entity has a public setter on a property.

## Rule description

Entities should encapsulate state changes via methods, not public setters. Public setters bypass business logic and invariant validation.

## How to fix violations

Use private setters and methods for state changes:

```csharp
// Bad - public setters
public class User : Entity
{
    public string Email { get; set; }  // Can be set to invalid value
    public bool IsActive { get; set; }
}

// Good - encapsulated state
public class User : Entity
{
    public string Email { get; private set; }
    public bool IsActive { get; private set; }

    public void ChangeEmail(string newEmail)
    {
        // Validation and business rules
        if (!EmailValidator.IsValid(newEmail))
            throw new DomainException("Invalid email format");

        Email = newEmail;
        AddDomainEvent(new EmailChangedEvent(Id, newEmail));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
    }
}
```

## Code fix

A code fix is available that changes the setter to `private set`.

## When to suppress

Suppress for:
- Simple value holder entities without invariants
- EF Core entity configurations that need public setters for materialization

## Related rules

- [MDYDDD001](MDYDDD001.md) - Mutable collection exposed
