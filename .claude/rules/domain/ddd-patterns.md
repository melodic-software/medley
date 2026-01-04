---
paths:
  - "**/Domain/**/*.cs"
---

<!-- ~750 tokens -->

# Domain-Driven Design Patterns

Apply DDD patterns for modules with complex business logic. For simple CRUD, use anemic models.

## When to Apply DDD

| Complexity | Approach |
|------------|----------|
| Simple CRUD (users, settings) | Anemic model + direct DbContext |
| Business rules, invariants | Rich domain model with DDD patterns |
| Complex workflows | Aggregates + domain events |

## Tactical Patterns

### Aggregate Root

Entry point for object clusters. Enforces invariants across the aggregate.

```csharp
public class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money Total => _items.Sum(i => i.Subtotal);

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify placed order");

        var item = _items.Find(i => i.ProductId == product.Id);
        if (item is not null)
            item.IncreaseQuantity(quantity);
        else
            _items.Add(new OrderItem(product, quantity));

        AddDomainEvent(new OrderItemAddedEvent(Id, product.Id, quantity));
    }

    public void Place()
    {
        if (!_items.Any())
            throw new DomainException("Cannot place empty order");

        Status = OrderStatus.Placed;
        AddDomainEvent(new OrderPlacedEvent(Id, Total));
    }
}
```

**Rules**:
- All changes go through aggregate root methods
- Never expose mutable collections
- Validate invariants in methods, not setters

### Entity

Objects with identity that persists over time.

```csharp
public class OrderItem : Entity<OrderItemId>
{
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Subtotal => UnitPrice * Quantity;

    internal OrderItem(Product product, int quantity)
    {
        ProductId = product.Id;
        UnitPrice = product.Price;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int amount) => Quantity += amount;
}
```

### Value Object

Immutable objects defined by their attributes, no identity.

```csharp
public record Money(decimal Amount, string Currency)
{
    public static Money USD(decimal amount) => new(amount, "USD");

    public static Money operator *(Money money, int multiplier)
        => money with { Amount = money.Amount * multiplier };

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot add different currencies");
        return left with { Amount = left.Amount + right.Amount };
    }
}

public record Address(string Street, string City, string PostalCode, string Country);
```

**Use records**: Gives immutability, equality, and `with` expressions for free.

### Domain Event

Notification that something significant happened in the domain.

```csharp
public record OrderPlacedEvent(OrderId OrderId, Money Total) : IDomainEvent;

// Raised in aggregate, dispatched after persistence
public abstract class AggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Domain Service

Stateless operations that don't belong to a single aggregate.

```csharp
public class PricingService(ITaxCalculator taxCalculator, IDiscountPolicy discountPolicy)
{
    public Money CalculateFinalPrice(Order order, Customer customer)
    {
        var subtotal = order.Total;
        var discount = discountPolicy.CalculateDiscount(order, customer);
        var tax = taxCalculator.CalculateTax(subtotal - discount, customer.Address);
        return subtotal - discount + tax;
    }
}
```

### Repository

Abstraction for aggregate persistence. Defined in Domain, implemented in Infrastructure.

```csharp
// Domain layer - interface
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    void Remove(Order order);
}

// Infrastructure layer - implementation
public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct)
        => await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
        => await context.Orders.AddAsync(order, ct);

    public void Remove(Order order) => context.Orders.Remove(order);
}
```

## Bounded Contexts

Each module = one bounded context with its own ubiquitous language.

| Module | "Customer" means |
|--------|-----------------|
| Sales | Prospect with purchase history |
| Shipping | Delivery address and preferences |
| Billing | Payment methods and invoices |

**Don't unify**: Same term, different meaning. Accept domain-specific naming.

## Prohibited

- Anemic domain model for complex business logic
- Exposing aggregate internals (mutable collections, setters)
- Repository methods returning `IQueryable<T>`
- Domain logic in application layer handlers
- Cross-aggregate references by object (use IDs instead)
