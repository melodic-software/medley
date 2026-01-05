# Chore: duplication-audit

## Chore Description

Create a repeatable `/audit-duplication` command that performs a comprehensive, exhaustive audit of the entire Medley repository to detect duplicated content, consolidate to single sources of truth (SSOT), and update all references. The command uses plan mode for strategy design, parallel subagents for efficient execution, and TODO list tracking for visibility. This ensures DRY principles are maintained, magic strings/numbers are replaced with constants, and documentation has clear canonical sources.

## Relevant Files

### Files to Create

- `.claude/commands/audit-duplication.md` - The slash command definition with YAML frontmatter

### Files to Modify (Examples of Duplication Targets)

**Documentation (SSOT consolidation):**
- `docs/architecture/shared-kernel.md` - Canonical SharedKernel documentation
- `src/SharedKernel/CLAUDE.md` - Should reference docs/ instead of duplicating
- `src/Modules/CLAUDE.md` - Should reference docs/architecture/module-patterns.md
- `src/CLAUDE.md` - Infrastructure patterns (may have overlap with docs/)
- `CLAUDE.md` (root) - Hub file referencing all others

**Code (Constants extraction candidates):**
- `src/SharedKernel/SharedKernel.Analyzers/DiagnosticIds.cs` - Diagnostic ID constants
- `src/SharedKernel/SharedKernel.Analyzers/DiagnosticCategories.cs` - Category constants
- Module-specific constants scattered across `src/Modules/**/*.cs`

**Configuration (Central management):**
- `Directory.Packages.props` - Central package version management
- `*.csproj` files - Should reference central packages
- `.editorconfig` - Code style (single source)

### Files to Reference (Detection Patterns)

- `.claude/rules/code-style/constants.md` - Constants organization patterns
- `.claude/rules/architecture/layer-dependencies.md` - Module boundary rules
- `docs/architecture/project-structure.md` - Overall structure context

## Step by Step Tasks

### Phase 1: Command Creation (Complete)

1. Create `.claude/commands/` directory structure
2. Write `audit-duplication.md` command with proper YAML frontmatter:
   - `description` field for discoverability
   - `argument-hint` for `--mode quick|full|report-only`
3. Define execution strategy using plan mode
4. Specify 6 parallel audit categories with subagent delegation

### Phase 2: Plan Mode Strategy Design

5. When `/audit-duplication` is invoked, enter plan mode via EnterPlanMode tool
6. Explore codebase to identify specific duplication instances
7. Create detailed audit plan with:
   - File-by-file audit scope for each category
   - Detection patterns customized to this codebase
   - Consolidation decisions (which file becomes SSOT)
   - Reference update strategy

### Phase 3: Parallel Audit Execution

8. Spawn **Documentation Auditor** Task agent:
   - Scope: `docs/**/*.md`, `**/CLAUDE.md`, `README.md`
   - Goal: Find overlapping documentation content
   - Output: JSON findings with file pairs and overlap percentage

9. Spawn **Code Pattern Auditor** Task agent:
   - Scope: `src/**/*.cs` (excluding tests)
   - Goal: Find repeated code blocks, utility methods, helper classes
   - Output: JSON findings with code snippets and locations

10. Spawn **Constants Auditor** Task agent:
    - Scope: `src/**/*.cs`
    - Goal: Find magic strings, magic numbers, hardcoded values
    - Output: JSON findings with literal values and suggested constant names

11. Spawn **Module Boundary Auditor** Task agent:
    - Scope: `src/Modules/**/*.cs`, `src/SharedKernel/**/*.cs`
    - Goal: Identify cross-module duplication, SharedKernel extraction candidates
    - Output: JSON findings with module pairs and extraction recommendations

12. Spawn **Test Duplication Auditor** Task agent:
    - Scope: `tests/**/*.cs`
    - Goal: Find duplicated test fixtures, setup code, helper methods
    - Output: JSON findings with test file locations and shared patterns

13. Spawn **Configuration Auditor** Task agent:
    - Scope: `*.json`, `*.yaml`, `*.props`, `*.targets`, `*.csproj`
    - Goal: Find duplicated settings, package versions, build configuration
    - Output: JSON findings with config keys and consolidation targets

### Phase 4: Scoring and False Positive Filtering

14. Aggregate all subagent findings into unified report
15. For each finding, calculate:
    - Similarity percentage (AST-based for code, Jaccard for docs)
    - Confidence level (High 90-100%, Medium 70-89%, Low 50-69%, Skip <50%)
16. Apply automatic exclusions for known false positive patterns:
    - Test fixtures (`tests/**/Fixtures/**`)
    - Generated code (`*.g.cs`, `*.Designer.cs`)
    - Module-scoped DDD aggregates (cross-module Domain layer)
    - Lock files and per-project configs
17. Check against previous decisions in `specs/duplication-audit-decisions.json`
18. Apply module boundary heuristics (same class name different namespace = intentional)

### Phase 5: Interactive Validation Checkpoint

19. Present findings summary via AskUserQuestion:
    - HIGH confidence count (will auto-consolidate)
    - MEDIUM confidence count (needs review)
    - LOW confidence count (report only)
20. Get user decision on validation approach:
    - [A] Approve HIGH, review MEDIUM one-by-one
    - [H] Approve HIGH only, skip MEDIUM
    - [R] Review all findings individually
    - [S] Skip to report-only
21. For each MEDIUM finding (if reviewing):
    - Show file locations and similarity percentage
    - Show context (module boundary, same interface, etc.)
    - Show key differences between files
    - Ask: [K]eep separate / [C]onsolidate / [S]kip
22. Log all decisions to `specs/duplication-audit-decisions.json` for future runs

### Phase 6: Consolidation Analysis

23. For **approved findings only**, classify:
    - **Accidental**: Should consolidate
    - **Intentional**: Module isolation (document)
    - **Derived**: Should reference SSOT
24. Determine consolidation targets for each approved finding
25. Plan reference updates with specific edit operations

### Phase 7: Execute Consolidation

26. Update documentation files to use SSOT references:
    - Add "See [docs/...] for complete documentation" patterns
    - Remove duplicated content from CLAUDE.md files
    - Preserve module-specific additions only

27. Extract constants from magic literals:
    - Create feature-scoped constant classes
    - Update usages to reference constants
    - Follow naming conventions from rules

28. Consolidate configuration:
    - Ensure packages use central version management
    - Extract common build settings to Directory.Build.props

### Phase 8: Validation and Report

29. Run `dotnet build --configuration Release` to verify no broken references
30. Run `dotnet test --configuration Release` to ensure tests pass
31. Generate final audit report at `specs/duplication-audit-report-{timestamp}.md`
32. Save audit decisions to `specs/duplication-audit-decisions.json`
33. Mark TODO items as complete

## Validation Commands

Verify the command was created correctly:
```bash
# Verify command file exists with proper frontmatter
cat .claude/commands/audit-duplication.md | head -10
```

Verify command is discoverable:
```bash
# In Claude Code, run /audit-duplication --help or just /audit-dup<tab>
```

After running the audit:
```bash
# Build verification
dotnet build --configuration Release

# Test verification
dotnet test --configuration Release --verbosity normal

# Check for audit report
ls specs/duplication-audit-report-*.md
```

## Notes

### False Positive Prevention

The command includes multiple layers of false positive prevention:

1. **Confidence scoring** - Only HIGH confidence (90%+) auto-consolidates
2. **Known exclusion patterns** - Test fixtures, generated code, lock files auto-excluded
3. **Module boundary heuristics** - Same class name in different modules defaults to "intentional"
4. **Interactive checkpoint** - User reviews MEDIUM confidence before any action
5. **Audit trail** - Previous decisions remembered for future runs (`--use-decisions`)
6. **Similarity thresholds** - Configurable via `--similarity N`, `--exact-only`, `--fuzzy`

### Module Boundary Considerations

When evaluating cross-module duplication, consider:
- **SharedKernel candidates**: Code used by 3+ modules with identical logic
- **Intentional isolation**: Module-specific implementations that happen to be similar
- **Contracts pattern**: Shared DTOs belong in `*.Contracts` projects, not SharedKernel

### Documentation Hierarchy

The SSOT hierarchy for documentation is:
1. `docs/architecture/*.md` - Canonical reference documentation
2. `CLAUDE.md` (root) - Hub pointing to detailed docs
3. `src/*/CLAUDE.md` - Context-specific guidance that references canonical docs

### Analyzer Rules Documentation

The `docs/analyzers/rules/` directory contains individual rule documentation. These should:
- Reference the central `docs/analyzers/rule-registry.md` for the master list
- Not duplicate content between rule files
- Use consistent structure across all rule files

### Token Budget Awareness

The CLAUDE.md files have token budget comments (e.g., `<!-- ~1,200 tokens -->`). When consolidating:
- Keep CLAUDE.md files lean (under 1,500 tokens)
- Move detailed content to `docs/`
- Use progressive disclosure pattern

### Edge Cases

1. **Generated code**: Skip `obj/`, `bin/`, generated analyzers
2. **Third-party content**: Don't modify `packages.lock.json`, external dependencies
3. **Test data**: Test fixtures may have intentional duplication for isolation
4. **Examples in docs**: Code examples in markdown may legitimately duplicate actual code

### Related Work

After this audit, consider:
- Creating a pre-commit hook to detect new duplication
- Adding a CI check for magic strings
- Automating SSOT reference verification
