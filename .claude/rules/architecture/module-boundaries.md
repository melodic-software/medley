---
paths:
  - "src/Modules/**"
---

<!-- ~400 tokens -->

# Module Boundaries

Medley uses a Modular Monolith architecture. Each module is self-contained with strict boundaries.

## Rules

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

## Integration Events

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

## Anti-Corruption Layer

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

## Prohibited

- Direct cross-module database queries
- Sharing DbContext across module boundaries
- Referencing non-Contracts projects from other modules
- Synchronous cross-module calls in request path (use events)
