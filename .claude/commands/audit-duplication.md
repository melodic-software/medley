---
description: Perform comprehensive duplication audit across the entire repository to establish single source of truth (SSOT) and eliminate redundancy
argument-hint: [--mode quick|full|report-only] [--similarity N] [--exact-only] [--skip-validation]
---

# Duplication Audit Command

**Purpose:** Perform a comprehensive, exhaustive audit of the entire repository to detect duplicated content, consolidate to single sources of truth, and update all references.

## Command Arguments

**Mode:**
- `--mode quick`: Fast scan focusing on high-impact duplication patterns
- `--mode full`: Complete folder-by-folder, file-by-file audit (default)
- `--mode report-only`: Generate findings without making changes

**Validation:**
- `--similarity N`: Set similarity threshold (default: 80 for code, 70 for docs)
- `--exact-only`: Only report 100% exact matches (fewest false positives)
- `--fuzzy`: Include 50%+ matches (more findings, more false positives)
- `--skip-validation`: Auto-approve HIGH confidence, skip interactive checkpoint
- `--use-decisions`: Load previous decisions from `specs/duplication-audit-decisions.json`

Arguments received: $ARGUMENTS

## Execution Strategy

This command uses **plan mode** to design the audit strategy, then executes with parallel subagents for efficiency.

### Phase 1: Enter Plan Mode and Design Audit Strategy

1. **Activate plan mode** via EnterPlanMode tool
2. **Create comprehensive audit plan** covering:
   - All directories and files in the repository
   - Detection patterns for each duplication type
   - Consolidation decisions and SSOT locations
   - Reference update strategy

### Phase 2: Execute Parallel Audit (After Plan Approval)

Spawn multiple Task agents in parallel to divide and conquer:

| Agent | Scope | Focus |
|-------|-------|-------|
| **Documentation Auditor** | `docs/**`, `*.md`, `CLAUDE.md` files | Duplicated documentation, overlapping content between docs/ and inline CLAUDE.md |
| **Code Pattern Auditor** | `src/**/*.cs` | Repeated code blocks, utility methods, helper classes across modules |
| **Constants Auditor** | `src/**/*.cs` | Magic strings, magic numbers, hardcoded values that should be constants |
| **Module Boundary Auditor** | `src/Modules/**`, `src/SharedKernel/**` | Cross-module duplication that should move to SharedKernel vs intentional isolation |
| **Test Duplication Auditor** | `tests/**/*.cs` | Duplicated test fixtures, helper methods, setup code |
| **Configuration Auditor** | `*.json`, `*.yaml`, `*.props`, `*.targets` | Duplicated build settings, package versions, configuration values |

### Phase 3: Consolidation Analysis

For each duplication found:

1. **Classify duplication type:**
   - **Accidental** - Same content, should be consolidated
   - **Intentional** - Module isolation requires separate copies (document why)
   - **Derived** - One file should reference another (SSOT pattern)

2. **Determine consolidation target:**
   - Use existing file if appropriate (e.g., `docs/architecture/shared-kernel.md` as SSOT)
   - Create new shared file if none exists (e.g., new constants class in SharedKernel)
   - For module-boundary cases, evaluate SharedKernel extraction

3. **Plan reference updates:**
   - Documentation: Add cross-references or imports
   - Code: Extract to shared library, use constants/enums
   - Configuration: Use Directory.Build.props or central package management

## False Positive Prevention & Validation

### Confidence Scoring

Each finding is assigned a confidence level:

| Score | Label | Meaning | Action |
|-------|-------|---------|--------|
| 90-100 | **High** | Exact or near-exact match, clearly accidental | Auto-consolidate (with `--mode full`) |
| 70-89 | **Medium** | Similar content, likely duplication | Present for review before action |
| 50-69 | **Low** | Partial overlap, may be intentional | Report only, recommend manual review |
| <50 | **Skip** | Minimal similarity, likely false positive | Exclude from report |

### Known False Positive Patterns (Auto-Exclude)

These patterns are **excluded by default** as they represent legitimate duplication:

```yaml
exclusions:
  # Test fixtures - isolation is intentional
  - pattern: "tests/**/Fixtures/**"
    reason: "Test fixtures intentionally isolated per test project"

  # Module boundary implementations
  - pattern: "src/Modules/*/Domain/**"
    cross_module: true
    reason: "DDD aggregates are intentionally module-scoped"

  # Interface implementations
  - pattern: "I*.cs implementations across modules"
    reason: "Same interface, different implementations is correct"

  # Generated code
  - pattern: "**/obj/**", "**/bin/**", "*.g.cs", "*.Designer.cs"
    reason: "Generated code excluded"

  # Lock files
  - pattern: "packages.lock.json"
    reason: "Lock files are per-project by design"

  # Constants that SHOULD be duplicated
  - pattern: "SchemaName constants in module DbContexts"
    reason: "Each module owns its schema name"
```

### Similarity Thresholds

Configurable via arguments:

```
--similarity-threshold 80    # Default: 80% for code, 70% for docs
--exact-only                 # Only report 100% matches
--fuzzy                      # Include 50%+ matches (more false positives)
```

**Similarity calculation:**
- **Code**: Normalized AST comparison (ignores whitespace, comments, variable names)
- **Documentation**: Sentence-level Jaccard similarity with stop-word removal
- **Configuration**: Key-value pair matching

### Interactive Validation Checkpoint

Before making ANY changes, present findings for user approval:

```
┌─────────────────────────────────────────────────────────────────┐
│ DUPLICATION AUDIT - VALIDATION CHECKPOINT                       │
├─────────────────────────────────────────────────────────────────┤
│ Found: 12 High, 8 Medium, 23 Low confidence findings            │
│                                                                  │
│ HIGH CONFIDENCE (will consolidate):                              │
│  [1] docs/shared-kernel.md ↔ src/SharedKernel/CLAUDE.md (94%)   │
│  [2] DiagnosticIds duplicated in 3 files (100%)                 │
│  ...                                                             │
│                                                                  │
│ MEDIUM CONFIDENCE (review required):                             │
│  [3] UserValidator.cs similar in Auth + Users modules (78%)     │
│      → Likely intentional: module boundary                       │
│  ...                                                             │
├─────────────────────────────────────────────────────────────────┤
│ Options:                                                         │
│  [A] Approve all HIGH, review MEDIUM one-by-one                 │
│  [H] Approve HIGH only, skip MEDIUM                             │
│  [R] Review all findings one-by-one                             │
│  [S] Skip to report-only (no changes)                           │
│  [C] Cancel audit                                                │
└─────────────────────────────────────────────────────────────────┘
```

Use **AskUserQuestion** tool to present this checkpoint.

### Per-Finding Validation

For MEDIUM confidence findings, ask:

```
Finding #3: UserValidator.cs
├── Location A: src/Modules/Auth/Application/Validators/UserValidator.cs
├── Location B: src/Modules/Users/Application/Validators/UserValidator.cs
├── Similarity: 78%
├── Context: Cross-module, similar validation logic
│
├── Differences:
│   - Auth: Validates login credentials, password policy
│   - Users: Validates profile updates, email uniqueness
│
└── Recommendation: INTENTIONAL (module boundary isolation)

Action? [K]eep separate / [C]onsolidate / [S]kip
```

### Module Boundary Heuristics

Apply special rules for cross-module findings:

| Pattern | Default Classification | Rationale |
|---------|----------------------|-----------|
| Same class name, different namespace | **Intentional** | Bounded context isolation |
| Identical utility method | **Consolidate** | Extract to SharedKernel |
| Similar domain entity | **Intentional** | DDD aggregate boundaries |
| Shared DTO in Contracts | **Correct** | Already using SSOT pattern |
| Duplicated EF configuration | **Review** | May need base configuration |

### Audit Trail

Every decision is logged to `specs/duplication-audit-decisions.json`:

```json
{
  "auditId": "2026-01-04-175500",
  "findings": [
    {
      "id": 1,
      "files": ["docs/shared-kernel.md", "src/SharedKernel/CLAUDE.md"],
      "similarity": 94,
      "confidence": "high",
      "classification": "accidental",
      "decision": "consolidate",
      "decidedBy": "auto",
      "ssotTarget": "docs/architecture/shared-kernel.md",
      "timestamp": "2026-01-04T17:55:30Z"
    },
    {
      "id": 3,
      "files": ["Auth/UserValidator.cs", "Users/UserValidator.cs"],
      "similarity": 78,
      "confidence": "medium",
      "classification": "intentional",
      "decision": "keep_separate",
      "decidedBy": "user",
      "reason": "Module boundary isolation - different validation concerns",
      "timestamp": "2026-01-04T17:56:12Z"
    }
  ]
}
```

This audit trail:
- Prevents re-flagging known intentional duplications
- Documents why decisions were made
- Speeds up future audits (skip already-decided items)

---

## Detection Patterns

### Documentation Duplication

```
Pattern: Similar headings, code blocks, or explanatory text across:
- docs/architecture/*.md vs src/*/CLAUDE.md
- Multiple CLAUDE.md files with overlapping guidance
- README.md vs docs/ content
```

**SSOT Strategy:** `docs/` is canonical, CLAUDE.md files should reference docs/ with "See [docs/...] for complete documentation"

### Code Duplication

```
Pattern: Methods/classes appearing in multiple modules:
- Extension methods
- Utility/helper classes
- Guard clauses
- Mapping code
```

**SSOT Strategy:**
- Cross-module utility code -> `SharedKernel/`
- Module-specific but repeated -> Extract to module-level shared class
- Intentional duplication (module isolation) -> Document with `// Intentional: Module boundary isolation`

### Magic Strings/Numbers

```
Pattern: Literal values that should be constants:
- Schema names: "Users", "Orders"
- Configuration keys
- Error codes/messages
- Regex patterns
- Timeout values
```

**SSOT Strategy:** Use feature-scoped constants classes per .claude/rules/code-style/constants.md

### Configuration Duplication

```
Pattern: Repeated settings across:
- Multiple .csproj files
- appsettings.*.json files
- Build configuration
```

**SSOT Strategy:**
- Package versions -> Directory.Packages.props (central package management)
- Build settings -> Directory.Build.props
- Runtime config -> Environment-specific appsettings with shared base

## Output Artifacts

### Audit Report (Always Generated)

Create `specs/duplication-audit-report-{timestamp}.md` with:

1. **Executive Summary** - Total findings, severity breakdown
2. **Findings by Category** - Grouped by duplication type
3. **Consolidation Recommendations** - Specific actions with file paths
4. **Module Boundary Analysis** - SharedKernel extraction candidates
5. **Deferred Items** - Intentional duplication that should remain

### Changes Made (Full/Quick Mode)

1. **Consolidated files** - List of SSOT files created/updated
2. **Updated references** - Files modified to point to SSOT
3. **Constants extracted** - New constant classes created
4. **Git diff summary** - Overview of all changes

## Workflow Steps

### Step 1: Initialize TODO List

Create tracking todos for each audit phase using TodoWrite tool.

### Step 2: Spawn Parallel Auditors

Use Task tool to spawn 6 parallel subagents (one per audit category).

**Example Task invocation:**
```
Task(
  subagent_type: "general-purpose",
  description: "Audit docs duplication",
  prompt: "Audit all documentation files in docs/ and compare with CLAUDE.md files throughout the repo. Find overlapping content, identify canonical source, report findings as structured JSON."
)
```

### Step 3: Aggregate Findings

Collect results from all subagents into unified findings structure.

### Step 4: Score and Classify Findings

For each finding:
1. Calculate similarity percentage
2. Assign confidence level (High/Medium/Low/Skip)
3. Check against known false positive patterns (auto-exclude)
4. Check against previous decisions in `specs/duplication-audit-decisions.json`
5. Apply module boundary heuristics

### Step 5: Interactive Validation Checkpoint

**Unless `--skip-validation` is passed:**

1. Present summary of findings grouped by confidence level
2. Use **AskUserQuestion** tool to get user decision:
   - Approve all HIGH confidence?
   - Review MEDIUM confidence one-by-one?
   - Skip to report-only?
3. For each MEDIUM finding requiring review:
   - Show file locations, similarity %, context
   - Show detected differences
   - Ask: Keep separate / Consolidate / Skip
4. Log all decisions to audit trail JSON

### Step 6: Generate Consolidation Plan

Based on **approved** findings only, create specific consolidation actions with:
- Source files (to be consolidated from)
- Target file (SSOT)
- Reference updates needed
- Validation steps

### Step 7: Execute Consolidation (if not report-only)

Apply changes using Edit/Write tools, updating references.

### Step 8: Validate

- Verify all references resolve correctly
- Run `dotnet build` to ensure no broken references
- Verify documentation links work

## Important Considerations

### Module Boundary Decisions

When code appears in multiple modules, evaluate:

1. **Is it truly shared logic?** -> Move to SharedKernel
2. **Is it module-specific but similar?** -> Keep separate (intentional duplication)
3. **Is it a candidate for extraction?** -> Note in SharedKernel backlog

### Documentation Cross-References

Use this pattern for CLAUDE.md files referencing docs/:

```markdown
# Topic Name

Context-specific guidance for [topic].

See [docs/architecture/topic.md](../../docs/architecture/topic.md) for complete documentation.

---

## Quick Reference (Module-Specific)

[Only include module-specific additions here]
```

### Constants Organization

Follow project conventions from `.claude/rules/code-style/constants.md`:

```csharp
// Feature-scoped (preferred)
public static class OrderConstants
{
    public const int MaxItemsPerOrder = 100;
}

// Module-level
public static class UsersModuleConstants
{
    public const string SchemaName = "Users";
}
```

## Validation Commands

After consolidation, run:

```bash
# Verify build succeeds
dotnet build --configuration Release

# Verify tests pass
dotnet test --configuration Release --verbosity normal

# Check for broken markdown links (if tool available)
# markdown-link-check docs/**/*.md
```

## Begin Execution

NOW: Enter plan mode to design the specific audit strategy for this repository.

Use EnterPlanMode tool, then create a detailed plan in `specs/chore-duplication-audit.md` covering:

1. Specific files to audit in each category
2. Expected duplication patterns based on codebase exploration
3. Consolidation targets (existing files or new files to create)
4. Order of operations for changes
5. Validation checkpoints
