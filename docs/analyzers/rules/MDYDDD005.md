# MDYDDD005: Value object with identity

| Property | Value |
|----------|-------|
| **Rule ID** | MDYDDD005 |
| **Category** | Medley.DDD |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A class inheriting from `ValueObject` has an `Id` property.

## Rule description

Value objects are defined by their attributes, not identity. If something has identity, it should be an Entity, not a Value Object.

Use C# records for value objects to get built-in equality by value.

## How to fix violations

Remove the Id property or change to an Entity:

```csharp
// Bad - value object with identity
public class Address : ValueObject
{
    public Guid Id { get; }  // Identity in a value object
    public string Street { get; }
    public string City { get; }
}

// Good - record as value object (recommended)
public record Address(string Street, string City, string PostalCode);

// Good - traditional value object
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }
}

// If identity is needed, it should be an Entity
public class AddressEntity : Entity<AddressId>
{
    public string Street { get; private set; }
    public string City { get; private set; }
}
```

## When to suppress

This rule should not be suppressed. Either remove the Id or convert to an Entity.

## Related rules

- [MDYDDD004](MDYDDD004.md) - Aggregate references by object
