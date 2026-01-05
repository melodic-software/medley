<!-- Last reviewed: 2026-01-04 -->
<!-- ~400 tokens -->
<!-- Lazy-loaded: Only included when working in src/ Infrastructure projects -->

# Infrastructure Patterns

Context-specific guidance for Infrastructure layer development.

## Documentation Hub

Detailed patterns are documented in `docs/architecture/`:

| Topic | Location |
|-------|----------|
| Logging & Observability | [docs/architecture/logging-observability.md](../docs/architecture/logging-observability.md) |
| EF Core Patterns | [docs/architecture/ef-core-patterns.md](../docs/architecture/ef-core-patterns.md) |
| Async/Await | [docs/architecture/async-patterns.md](../docs/architecture/async-patterns.md) |
| Dependency Injection | [docs/architecture/dependency-injection.md](../docs/architecture/dependency-injection.md) |

---

## Quick Reference

### Logging

Use source-generated `[LoggerMessage]` for performance:

```csharp
[LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} created")]
public static partial void UserCreated(this ILogger logger, Guid userId);
```

### EF Core Data Access

| Operation | Pattern |
|-----------|---------|
| Reads (Queries) | Direct DbContext with `AsNoTracking()` |
| Writes (Commands) | Repository abstraction for DDD |
| Blazor | `IDbContextFactory<T>` (not DbContext) |

### Aspire Observability

```csharp
builder.AddServiceDefaults();  // Configures OpenTelemetry automatically
```

---

## Prohibited

- String interpolation in log messages
- Logging sensitive data (PII, credentials)
- Direct DbContext injection in Blazor components
- `Include()` chains longer than 2 levels
- `.Result` or `.Wait()` on Tasks
- `async void` except for event handlers
