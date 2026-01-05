# MDYDDD001: Mutable collection exposed

| Property | Value |
|----------|-------|
| **Rule ID** | MDYDDD001 |
| **Category** | Medley.DDD |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | Yes |

## Cause

An aggregate exposes a mutable collection (e.g., `List<T>`, `IList<T>`, `ICollection<T>`).

## Rule description

Aggregates must protect their invariants. Exposing mutable collections allows external code to bypass aggregate methods and violate business rules.

Collections should be exposed as `IReadOnlyCollection<T>` or using `AsReadOnly()`.

## How to fix violations

Encapsulate the collection:

```csharp
// Bad - mutable collection exposed
public class Order : AggregateRoot
{
    public List<OrderItem> Items { get; set; } = [];  // External code can modify
}

// Good - encapsulated collection
public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(Product product, int quantity)
    {
        // Business rules enforced here
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        _items.Add(new OrderItem(product, quantity));
    }
}
```

## Code fix

A code fix is available that changes the property type to `IReadOnlyCollection<T>` and adds a private backing field.

## When to suppress

Suppress for DTOs or EF Core entity configurations that require mutable collections.

## Related rules

- [MDYDDD002](MDYDDD002.md) - Public setter on entity
