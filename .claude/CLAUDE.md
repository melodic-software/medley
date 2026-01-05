<!-- Last reviewed: 2026-01-04 -->
<!-- ~650 tokens -->
<!-- Lazy-loaded: Only included when working in .claude/rules/ directory -->
<!-- CANARY: RULES_META_V1_2026 -->

# Claude Code Rules Authoring Guide

This file provides guidance for creating and maintaining rules in `.claude/rules/`.

## Frontmatter Syntax

**IMPORTANT:** Use YAML array syntax due to known bugs with official string syntax.

```yaml
# ✅ Correct - YAML array syntax
---
paths:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---

# ❌ Incorrect - These are broken
paths: "src/**/*.cs"           # String syntax - #16038
paths: src/**/*.cs             # Unquoted - YAML parse error
paths: "**/*.{ts,tsx}"         # Brace expansion - #13905
```

| What Official Docs Say | What We Do Instead | Why |
|------------------------|-------------------|-----|
| `paths: "**/*.cs"` | YAML array syntax | String syntax broken (#16038) |
| `paths: "**/*.{ts,tsx}"` | Separate array entries | Brace expansion fails (#13905) |

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

## Prohibited

- String syntax for `paths:` field (use YAML array)
- Unquoted glob patterns in frontmatter
- Brace expansion in glob patterns (`{ts,tsx}`)
- Rules without token estimate comments
- Rules exceeding 800 tokens
- PascalCase or snake_case filenames (use kebab-case)
- Custom frontmatter fields beyond `paths:` (#13003)
