<!-- Last reviewed: 2026-01-04 -->
<!-- ~600 tokens -->

# GitHub Configuration

Context-specific guidance for GitHub repository configuration files.

## Quick Reference

| File/Directory | Purpose |
|----------------|---------|
| `workflows/` | GitHub Actions CI/CD pipelines |
| `ISSUE_TEMPLATE/` | Standardized issue forms |
| `dependabot.yml` | Automated dependency updates |
| `CODEOWNERS` | Code review ownership rules |
| `pull_request_template.md` | PR description template |
| `copilot-instructions.md` | GitHub Copilot AI guidance |

## Workflows

### Security Requirements

**SHA-pinned actions** - All action versions MUST use full commit SHA for supply chain security:

```yaml
# Good - SHA-pinned
- uses: actions/checkout@8e8c483db84b4bee98b60c0593521ed34d9990e8  # v6.0.1

# Bad - tag only
- uses: actions/checkout@v4
```

**Principle of least privilege** - Explicit permissions per workflow:

```yaml
permissions:
  contents: read
  packages: read
```

### .NET Build Pattern

Standard .NET workflow steps:

```yaml
- uses: actions/setup-dotnet@v5
  with:
    global-json-file: global.json
    cache: true
    cache-dependency-path: '**/packages.lock.json'
- run: dotnet restore
- run: dotnet build --configuration Release --no-restore
- run: dotnet test --configuration Release --no-build --verbosity normal
```

### Timeout Guidelines

| Workflow Type | Recommended Timeout |
|---------------|---------------------|
| Build/Test | 15 minutes |
| CodeQL Analysis | 45 minutes |
| Commit Validation | 5 minutes |

## Dependabot Configuration

### Update Groups

- `dotnet-minor-patch`: Minor/patch updates grouped (less noise)
- `dotnet-major`: Major updates separate (breaking changes need review)
- Security updates: Never grouped (immediate visibility)

### Schedule

Weekly on Mondays at 06:00 America/Chicago timezone.

## Issue Templates

### YAML Form Syntax

Use `.yml` extension with structured fields:

```yaml
name: Bug Report
description: Report a bug
labels: ["type:bug", "status:needs-triage"]
body:
  - type: textarea
    id: description
    attributes:
      label: Description
    validations:
      required: true
```

### Dropdowns with Validation

```yaml
- type: dropdown
  id: severity
  attributes:
    label: Severity
    options:
      - Low
      - Medium
      - High
      - Critical
  validations:
    required: true
```

## CODEOWNERS Patterns

### Syntax

```
# Default owner (catch-all, must be first)
* @melodic-software/core-team

# Path-specific ownership
/src/Modules/Auth/ @melodic-software/auth-team
/.github/ @melodic-software/core-team
*.md @melodic-software/docs-team
```

### Requirements

- Teams must exist in GitHub Organization
- Invalid references are silently ignored
- More specific patterns override general ones

## Conventional Commits Validation

PR titles and commits validated against pattern:

```regex
^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\([a-zA-Z0-9._-]+\))?(!)?: .+
```

### Valid Examples

```
feat: add user authentication
fix(api): handle null response
feat!: breaking change to auth flow
ci(deps): bump actions/checkout to v6
```

## Relationship to copilot-instructions.md

- `copilot-instructions.md` - Guidance for GitHub Copilot AI
- `CLAUDE.md` (this file) - Guidance for Claude Code AI

Both should remain aligned on:
- Commit conventions
- Code style preferences
- Architecture patterns
- Testing conventions

## Prohibited

- Tag-only action versions without SHA
- Broad permissions (prefer explicit minimal permissions)
- Secrets in workflow files (use GitHub Secrets)
- Disabling security workflows
- Empty catch blocks in workflow scripts
