# MDYARCH002: Cross-module database access detected

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH002 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

A module accesses database tables or schemas owned by another module directly.

## Rule description

Each module owns its database schema (e.g., `[Orders].[OrderItems]`, `[Users].[Profiles]`). Direct database access between modules bypasses the intended communication layer and creates hidden dependencies.

Cross-module data access should go through the owning module's public API (via Contracts).

## How to fix violations

Instead of querying another module's tables directly, use the module's public API:

```csharp
// Bad - direct database access across modules
var customer = await _context.Set<Customer>()
    .FromSqlRaw("SELECT * FROM [Customers].[Customers] WHERE Id = @id", id)
    .FirstOrDefaultAsync();

// Good - use integration events or query through the owning module
var customer = await _mediator.Send(new GetCustomerQuery(customerId));
```

For read-heavy scenarios, consider:
- Materialized views within your module (fed by integration events)
- Dedicated read models updated via event handlers

## When to suppress

Suppress only for:
- Reporting/analytics modules that need cross-module reads (with clear documentation)
- Migration scenarios during module extraction

## Related rules

- [MDYARCH001](MDYARCH001.md) - Cross-module direct reference
