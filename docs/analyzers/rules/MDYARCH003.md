# MDYARCH003: Domain layer references Application layer

| Property | Value |
|----------|-------|
| **Rule ID** | MDYARCH003 |
| **Category** | Medley.Architecture |
| **Severity** | Error |
| **Enabled** | Yes |

## Cause

The Domain layer contains a reference to the Application layer.

## Rule description

Clean Architecture requires the Domain layer to have no dependencies on outer layers. The Domain layer should contain only:
- Entities
- Value Objects
- Domain Events
- Repository interfaces (abstractions)
- Domain Services

The Domain layer must not depend on Application layer concerns like commands, queries, or handlers.

## How to fix violations

Move Application layer dependencies out of Domain:

```csharp
// Bad - Domain referencing Application
using MyModule.Application.Commands;

public class Order : AggregateRoot
{
    public void Process(CreateOrderCommand command) // Application type in Domain
    {
    }
}

// Good - Pure domain model
public class Order : AggregateRoot
{
    public void Process(OrderDetails details) // Domain type
    {
    }
}
```

## When to suppress

This rule should not be suppressed. Domain layer purity is fundamental to Clean Architecture.

To opt-out for vertical slice modules, use:

```csharp
[assembly: ModuleArchitecture(ModuleStyle.VerticalSlicesOnly)]
```

Or in `.editorconfig`:
```ini
[src/Modules/SimpleModule/**/*.cs]
dotnet_diagnostic.MDYARCH003.severity = none
```

## Related rules

- [MDYARCH004](MDYARCH004.md) - Domain references Infrastructure
- [MDYARCH005](MDYARCH005.md) - Application references Infrastructure
