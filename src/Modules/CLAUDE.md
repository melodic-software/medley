<!-- Last reviewed: 2026-01-04 -->
<!-- ~1,150 tokens -->
<!-- Lazy-loaded: Only included when working in src/Modules/ directory -->

# Module Development Patterns

Context-specific guidance for modular monolith development: module boundaries and domain-driven design.

---

## Module Boundaries

Medley uses a Modular Monolith architecture. Each module is self-contained with strict boundaries.

### Rules

1. **No cross-module direct references** - Modules communicate only through:
   - Integration events (async messaging)
   - Public contracts (shared DTOs in Contracts projects)
   - Module APIs (explicit public interfaces)

2. **Module structure** - Each module follows:
   ```
   src/Modules/{ModuleName}/
   ├── {ModuleName}.Application/     # Use cases, CQRS handlers
   ├── {ModuleName}.Domain/          # Entities, value objects, domain events
   ├── {ModuleName}.Infrastructure/  # EF Core, external services
   └── {ModuleName}.Contracts/       # Public DTOs, integration events
   ```

3. **Dependency direction** - Only depend on:
   - Your own module's layers (inward)
   - Shared kernel (`src/SharedKernel/`)
   - Other modules' `.Contracts` projects only

4. **Database isolation** - Each module owns its schema:
   - Use schema prefix: `[ModuleName].[TableName]`
   - Never query another module's tables directly

### Integration Events

Cross-module communication via strongly-typed events:

```csharp
// Define in Contracts project (e.g., Orders.Contracts)
public record OrderPlacedEvent(Guid OrderId, Guid CustomerId, decimal Total) : IIntegrationEvent;

// Publish from domain handler
public class PlaceOrderHandler(IEventPublisher publisher)
{
    public async Task Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        // ... create order
        await publisher.PublishAsync(new OrderPlacedEvent(order.Id, order.CustomerId, order.Total), ct);
    }
}

// Subscribe in another module (e.g., Notifications.Application)
public class OrderPlacedHandler : IIntegrationEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken ct)
    {
        await _notificationService.SendOrderConfirmationAsync(@event.CustomerId, ct);
    }
}
```

**Outbox pattern**: Use transactional outbox for reliable publishing:
```csharp
// Events saved in same transaction as domain changes
await _outbox.SaveAsync(events, ct);
await _unitOfWork.CommitAsync(ct);
// Background processor publishes from outbox
```

### Anti-Corruption Layer

When integrating with external systems or legacy modules:

```csharp
// ACL translates between external and internal models
public class LegacyCustomerAdapter(ILegacyCustomerApi legacy) : ICustomerService
{
    public async Task<Customer> GetCustomerAsync(CustomerId id, CancellationToken ct)
    {
        var legacyDto = await legacy.GetCustomer(id.Value);
        return MapToCustomer(legacyDto);  // Translation happens here
    }
}
```

**Purpose**: Isolate your domain from external models and terminology.

### Prohibited - Module Boundaries

- Direct cross-module database queries
- Sharing DbContext across module boundaries
- Referencing non-Contracts projects from other modules
- Synchronous cross-module calls in request path (use events)

---

## Domain-Driven Design Patterns

Apply DDD patterns for modules with complex business logic. For simple CRUD, use anemic models.

### When to Apply DDD

| Complexity | Approach |
|------------|----------|
| Simple CRUD (users, settings) | Anemic model + direct DbContext |
| Business rules, invariants | Rich domain model with DDD patterns |
| Complex workflows | Aggregates + domain events |

### Tactical Patterns

#### Aggregate Root

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

#### Entity

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

#### Value Object

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

#### Domain Event

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

#### Domain Service

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

#### Repository

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

### Bounded Contexts

Each module = one bounded context with its own ubiquitous language.

| Module | "Customer" means |
|--------|-----------------|
| Sales | Prospect with purchase history |
| Shipping | Delivery address and preferences |
| Billing | Payment methods and invoices |

**Don't unify**: Same term, different meaning. Accept domain-specific naming.

### Prohibited - DDD

- Anemic domain model for complex business logic
- Exposing aggregate internals (mutable collections, setters)
- Repository methods returning `IQueryable<T>`
- Domain logic in application layer handlers
- Cross-aggregate references by object (use IDs instead)
