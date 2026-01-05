---
paths:
  - "**/*.cs"
---

<!-- Last reviewed: 2026-01-04 -->
<!-- ~650 tokens -->

# Comments and Documentation

## Philosophy

Self-documenting code first. Comments explain "why", not "what".

## Comment Types

### 1. XML Documentation Comments (///)

**Required for**: Public types and members only (visible outside assembly)

**Purpose**:
- IntelliSense tooltips
- OpenAPI/Swagger documentation generation
- API reference documentation

**Format conventions**:
- `<summary>` - Start with verb phrase ("Gets...", "Creates...", "Validates...")
- `<param>` - Describe effect/purpose, not just type
- `<returns>` - Include for non-void methods
- `<exception>` - Document custom exceptions only
- `<inheritdoc/>` - Use for interface implementations

**Example**:
```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="id">The unique user identifier.</param>
/// <param name="ct">Cancellation token for the operation.</param>
/// <returns>The user if found; otherwise, a not found error.</returns>
public async Task<Result<UserDto>> GetUserByIdAsync(UserId id, CancellationToken ct)
```

### 2. Implementation Comments (//)

**Avoid unless**:
- Explaining non-obvious business logic ("why")
- Documenting workarounds with issue references
- Warning about non-intuitive behavior
- Regulatory/compliance requirements

**Never comment**:
- What the code does (let code speak)
- Redundant information (`// Increment counter` above `counter++`)
- Commented-out code (delete it, use source control)

### 3. TODO/FIXME/HACK Markers

**Required format**: `// TODO(username): #issue-number description`

**Examples**:
```csharp
// TODO(kyle): #123 Add retry logic for transient failures
// FIXME(kyle): #456 Race condition in concurrent access
// HACK(kyle): #789 Workaround for EF Core limitation, remove after v10
```

**Rules**:
- Must include author identifier
- Should reference issue number when applicable
- Remove after addressing (don't leave stale TODOs)

### 4. Region Comments

**Prohibited** - Use file/class organization instead

## What NOT to Document

| Element | Reason |
|---------|--------|
| Private members | Implementation detail, code should be clear |
| Internal members | Within-assembly use, team maintains directly |
| Test code | Test names are documentation |
| Generated code | Auto-generated, don't maintain manually |
| Obvious properties | `public string Name { get; set; }` is self-explanatory |

## Prohibited

- XML docs on private/internal members (noise, not consumed)
- Redundant comments restating code
- Commented-out code blocks
- `#region` directives
- Empty XML doc stubs (`/// <summary></summary>`)
- Disabling CS1591 on public APIs
