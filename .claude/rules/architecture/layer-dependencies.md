---
paths:
  - "src/**/*.cs"
---

<!-- ~150 tokens -->

# Clean Architecture Layer Dependencies

## Layer Hierarchy (innermost to outermost)

```
Domain → Application → Infrastructure → Presentation
```

## Rules

1. **Domain layer** - No dependencies on other layers
   - Contains: Entities, Value Objects, Domain Events, Repository interfaces
   - References: Only primitives and .NET BCL

2. **Application layer** - Depends only on Domain
   - Contains: Commands, Queries, Handlers, DTOs, Application Services
   - References: Domain layer, MediatR abstractions (via our interfaces)

3. **Infrastructure layer** - Implements abstractions from inner layers
   - Contains: EF Core DbContext, Repository implementations, External service clients
   - References: Domain, Application layers

4. **Presentation layer** - Entry point, depends on all layers for DI
   - Contains: API Controllers, Blazor components, Middleware
   - References: All layers (for composition root only)

## Prohibited

- Domain referencing Application, Infrastructure, or Presentation
- Application referencing Infrastructure or Presentation
- Any layer referencing implementation details of outer layers
