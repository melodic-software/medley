---
paths:
  - "**/*.cs"
---

<!-- Last reviewed: 2026-01-04 -->
<!-- ~150 tokens -->

# Dependency Injection Patterns

See [docs/architecture/dependency-injection.md](../../../docs/architecture/dependency-injection.md) for complete patterns.

## Quick Reference

| Lifetime | Use For |
|----------|---------|
| Singleton | Stateless, thread-safe (IConfiguration, IHttpClientFactory) |
| Scoped | Per-request (DbContext, repositories) |
| Transient | Lightweight, no shared state (validators, handlers) |

**Prefer primary constructors** for simple DI injection.

## Prohibited

- **Service locator pattern** - Injecting `IServiceProvider` into business logic
- **Captive dependencies** - Singleton holding Scoped service
- **Constructor over-injection** - More than 5-7 dependencies suggests SRP violation
- **Static service locator** - Never use static ServiceProvider access
