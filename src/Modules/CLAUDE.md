<!-- Last reviewed: 2026-01-04 -->
<!-- ~400 tokens -->
<!-- Lazy-loaded: Only included when working in src/Modules/ directory -->

# Module Development Patterns

Context-specific guidance for modular monolith development.

**Complete documentation:** [Module Patterns](../../docs/architecture/module-patterns.md)

---

## Quick Reference

### Module Structure

```
src/Modules/{ModuleName}/
├── {ModuleName}.Domain/          # Entities, value objects, domain events
├── {ModuleName}.Application/     # Features/{FeatureName}/ vertical slices
├── {ModuleName}.Infrastructure/  # EF Core DbContext, repositories
└── {ModuleName}.Contracts/       # PUBLIC: Integration events, DTOs
```

### Key Rules

1. **Module isolation** - Communicate only through Contracts and integration events
2. **Database schema** - Each module owns its schema: `[ModuleName].[TableName]`
3. **Dependency direction** - Only depend on SharedKernel and other modules' Contracts
4. **Integration events** - Use outbox pattern for reliable async communication

### Integration Events

```csharp
// In {Module}.Contracts
public record OrderPlacedEvent(Guid OrderId, decimal Total) : IIntegrationEvent;

// Publish
await publisher.PublishAsync(new OrderPlacedEvent(order.Id, order.Total), ct);

// Subscribe (different module)
public class OrderPlacedHandler : IIntegrationEventHandler<OrderPlacedEvent> { ... }
```

### Prohibited

- Direct cross-module database queries
- Referencing non-Contracts projects from other modules
- Synchronous cross-module calls in request path

---

## DDD Patterns

Apply for modules with complex business logic. See [Shared Kernel](../../docs/architecture/shared-kernel.md) for base classes.

| Pattern | When to Use |
|---------|-------------|
| AggregateRoot | Entry point enforcing invariants |
| Entity | Objects with identity |
| ValueObject | Immutable, defined by attributes (use records) |
| DomainEvent | Something significant happened |
| Repository | Aggregate persistence abstraction |

### Bounded Contexts

Each module = one bounded context. Same term can mean different things:

| Module | "Customer" means |
|--------|-----------------|
| Sales | Prospect with purchase history |
| Shipping | Delivery address and preferences |

### Prohibited - DDD

- Anemic domain model for complex logic
- Exposing aggregate internals
- Repository methods returning `IQueryable<T>`
- Cross-aggregate references by object (use IDs)
