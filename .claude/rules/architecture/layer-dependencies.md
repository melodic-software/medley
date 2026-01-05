---
paths:
  - "src/**/*.cs"
---

<!-- ~50 tokens -->

# Layer Dependencies

**SSOT:** [docs/architecture/project-structure.md](../../../docs/architecture/project-structure.md#layer-dependencies)

## Quick Reference

```
Domain -> Application -> Infrastructure -> Presentation
```

## Prohibited

- Domain referencing Application, Infrastructure, or Presentation
- Application referencing Infrastructure or Presentation
- Any layer referencing implementation details of outer layers
