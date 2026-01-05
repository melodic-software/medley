---
paths:
  - "**/*.cs"
---

<!-- Last reviewed: 2026-01-04 -->
<!-- ~150 tokens -->

# Async/Await Best Practices

See [docs/architecture/async-patterns.md](../../../docs/architecture/async-patterns.md) for complete patterns.

## Quick Reference

| Rule | Pattern |
|------|---------|
| CancellationToken | Accept in all async methods, pass downstream |
| Task vs ValueTask | Default to Task; ValueTask for hot paths with sync completion |
| ConfigureAwait | Use `ConfigureAwait(false)` in SharedKernel/library code |

## Prohibited

- **`.Result` or `.Wait()`** - Causes deadlocks
- **`async void`** - Except for event handlers
- **Fire-and-forget** without error handling
- **Ignoring CancellationToken** parameters
