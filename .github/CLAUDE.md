<!-- Last reviewed: 2026-01-04 -->
<!-- ~600 tokens -->
<!-- Lazy-loaded: Only included when working in .github/ directory -->

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
# Good - SHA-pinned with version comment
- uses: actions/checkout@8e8c483db84b4bee98b60c0593521ed34d9990e8  # v6.0.1
- uses: anthropics/claude-code-action@7145c3e0510bcdbdd29f67cc4a8c1958f1acfa2f  # v1.0.27

# Bad - tag only (vulnerable to supply chain attacks)
- uses: actions/checkout@v4
```

**Current action versions** (as of 2026-01-04):

| Action | Version | SHA |
|--------|---------|-----|
| actions/checkout | v6.0.1 | `8e8c483db84b4bee98b60c0593521ed34d9990e8` |
| actions/upload-artifact | v6.0.0 | `b7c566a772e6b6bfb58ed0dc250532a479d7789f` |
| actions/setup-dotnet | v5.0.1 | `2016bd2012dba4e32de620c46fe006a3ac9f0602` |
| actions/stale | v10.1.1 | `997185467fa4f803885201cee163a9f38240193d` |
| actions/labeler | v6.0.1 | `634933edcd8ababfe52f92936142cc22ac488b1b` |
| github/codeql-action | v4.31.9 | `5d4e8d1aca955e8d8589aabd499c5cae939e33c7` |
| anthropics/claude-code-action | v1.0.27 | `7145c3e0510bcdbdd29f67cc4a8c1958f1acfa2f` |
| martincostello/update-dotnet-sdk | v5.0.0 | `a832b148de803dd5bcba5dbedb4ca75f20c3a0c4` |
| webiny/action-conventional-commits | v1.3.0 | `8bc41ff4e7d423d56fa4905f6ff79209a78776c7` |

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

See inline comments in [dependabot.yml](dependabot.yml) for update groups and schedule configuration.

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

- `copilot-instructions.md` - Guidance for GitHub Copilot AI (code completion focused)
- `CLAUDE.md` (this file) - Guidance for Claude Code AI (agentic workflows)

**Aligned on development conventions** (keep synchronized):
- Commit message format (Conventional Commits)
- Code style preferences (C# 14, file-scoped namespaces, etc.)
- Architecture patterns (modular monolith, vertical slices)
- Testing conventions (xUnit v3, Shouldly)

**CLAUDE.md additionally covers** (not in copilot-instructions.md):
- GitHub Actions security (SHA pinning, permissions)
- Workflow configuration patterns
- CI/CD best practices

## Prohibited

- Tag-only action versions without SHA
- Broad permissions (prefer explicit minimal permissions)
- Secrets in workflow files (use GitHub Secrets)
- Disabling security workflows
- Empty catch blocks in workflow scripts
