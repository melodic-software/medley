<!-- Last reviewed: 2026-01-04 -->
<!-- ~300 tokens -->
<!-- Lazy-loaded: Only included when working in src/SharedKernel/ or Application layer -->

# SharedKernel Patterns

Context-specific guidance for SharedKernel development.

**Complete documentation:**
- [Shared Kernel](../../docs/architecture/shared-kernel.md) - DDD building blocks, Result pattern
- [CQRS Patterns](../../docs/architecture/cqrs-patterns.md) - Commands, queries, pipeline behaviors

---

## Quick Reference

### CQRS Rules

1. **Use MediatR directly** - No custom wrapper interfaces
2. **Commands return `Result<T>`** - Never throw for business failures
3. **Queries are read-only** - No side effects
4. **Vertical slices**: `Features/{FeatureName}/` groups command + handler + validator

### Result Pattern

```csharp
result.Map(x => transform(x))       // Transform success value
result.Bind(x => Validate(x))       // Chain operations returning Result<T>
result.Match(success, failure)      // Pattern match
```

### Validation Layers

| Layer | Responsibility | Tool |
|-------|---------------|------|
| API/Presentation | Input format | FluentValidation |
| Application | Business rules | Pipeline behavior |
| Domain | Invariants | Guard clauses |

### DDD Building Blocks

Core primitives in `SharedKernel/` (ZERO external dependencies):

| Type | Purpose |
|------|---------|
| Entity | Objects with identity |
| AggregateRoot | Consistency boundaries + domain events |
| ValueObject | Immutable, defined by attributes |
| DomainEvent | Something significant happened |
| Specification | Query patterns |

### Prohibited

- Throwing exceptions for business rule failures (use Result<T>)
- Side effects in query handlers
- Validation logic in endpoints
- External dependencies in SharedKernel core
