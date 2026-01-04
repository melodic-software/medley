<!-- Last reviewed: 2026-01-04 -->
<!-- ~800 tokens -->
<!-- Lazy-loaded: Only included when working in .claude/rules/ directory -->
<!-- CANARY: RULES_META_V1_2026 -->

# Claude Code Rules Authoring Guide

This file provides guidance for creating and maintaining rules in `.claude/rules/`.

## Deviations from Official Documentation

This project uses YAML array syntax instead of the documented string syntax due to known bugs.

| Aspect | Official Docs Say | We Do Instead | Why (Issue) |
|--------|-------------------|---------------|-------------|
| `paths:` syntax | `paths: "**/*.cs"` | `paths:`<br>`  - "**/*.cs"` | String syntax broken (#16038, #13905) |
| Brace expansion | `paths: "**/*.{ts,tsx}"` | Separate array entries | Reserved YAML chars fail (#13905) |

## Frontmatter Syntax

### Correct (Use This)

```yaml
---
paths:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---
```

### Incorrect (Documented But Broken)

```yaml
paths: "src/**/*.cs"           # String syntax - #16038
paths: src/**/*.cs             # Unquoted - YAML parse error
paths: "**/*.{ts,tsx}"         # Brace expansion - #13905
```

## Loading vs Applying (CRITICAL)

Per GitHub Issue #15710:

- **LOAD**: ALL rules in `.claude/rules/` load at startup (consumes tokens)
- **APPLY**: `paths:` only controls when rules are considered, not loaded

This is NOT lazy loading. For true lazy loading, use nested `CLAUDE.md` files in source directories.

## Glob Pattern Reference

| Pattern | Matches |
|---------|---------|
| `**/*.cs` | All C# files in any directory |
| `src/**/*.cs` | C# files under src/ |
| `**/Domain/**/*.cs` | Domain layer files |
| `tests/**/*.cs` | Test files |

Multiple patterns require separate array entries:

```yaml
paths:
  - "**/*.razor"
  - "**/*.razor.cs"
```

## File Structure Template

```markdown
---
paths:
  - "your/glob/**/*.pattern"
---

<!-- ~XXX tokens -->

# Rule Title

## Section
[Content]

## Prohibited
- [Anti-patterns]
```

## Project Conventions

- **Filenames**: kebab-case (`ef-core-patterns.md`)
- **Token budget**: 150-800 tokens per file
- **Organization**: Group by domain in subdirectories
- **Token comment**: Include `<!-- ~XXX tokens -->` estimate
- **Prohibited section**: Every rule should list anti-patterns

## Known Issues Reference

| Issue | Status | Impact | Workaround |
|-------|--------|--------|------------|
| [#16038](https://github.com/anthropics/claude-code/issues/16038) | Open | String `paths:` syntax broken | Use YAML array |
| [#13905](https://github.com/anthropics/claude-code/issues/13905) | Open | Unquoted globs fail YAML parse | Quote all patterns |
| [#15710](https://github.com/anthropics/claude-code/issues/15710) | Closed | `paths:` doesn't lazy-load | Use nested CLAUDE.md for lazy |
| [#13003](https://github.com/anthropics/claude-code/issues/13003) | Closed | Custom frontmatter stripped | Only use `paths:` field |
| [#13914](https://github.com/anthropics/claude-code/issues/13914) | Open | User rules don't load in VSCode | Use project rules or CLI |

## Validation Checklist

- [ ] Uses YAML array syntax for `paths:`
- [ ] All glob patterns are quoted strings
- [ ] Token estimate comment present
- [ ] Has "Prohibited" section for anti-patterns
- [ ] Follows kebab-case filename convention
- [ ] Under 800 tokens
