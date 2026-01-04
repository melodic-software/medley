<!-- Last reviewed: 2026-01-04 -->

# Copilot Instructions for Medley

## Project Overview

Medley is a .NET 10 modular monolith application using C# 14, following trunk-based development with squash merging.

## Commit Convention (REQUIRED)

All commits **must** follow [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Commit Types

| Type | Description | Example Scope |
|------|-------------|---------------|
| `feat` | New feature | `auth`, `blazor`, `api` |
| `fix` | Bug fix | module name |
| `docs` | Documentation only | - |
| `style` | Formatting, no code change | - |
| `refactor` | Code change, no new feature or fix | module name |
| `perf` | Performance improvement | module name |
| `test` | Adding/updating tests | module name |
| `chore` | Maintenance tasks | `deps` |
| `build` | Build system changes | - |
| `ci` | CI/CD changes | `deps` |

### Examples

```
feat(auth): add passkey authentication support
fix(blazor): resolve hydration issue on login page
docs: update README with deployment instructions
chore(deps): update NuGet packages to latest versions
ci(deps): bump actions/checkout from 4.1.0 to 4.2.0
```

### Commit Message Guidelines

- Use imperative mood: "add feature" not "added feature"
- Keep subject line under 72 characters
- Do not end subject with a period
- Separate subject from body with a blank line
- Reference issues in footer: `Closes #123`

## Code Style

- Follow [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use file-scoped namespaces (`namespace Foo;`)
- Prefer records for DTOs and value objects
- Use nullable reference types (NRTs enabled project-wide)
- All warnings are treated as errors (TreatWarningsAsErrors)
- Use primary constructors where appropriate (C# 12+)
- Prefer collection expressions (`[1, 2, 3]`) over initializers
- Use `field` keyword for property backing fields (C# 14)
- Use extension members for extending types with properties/static methods (C# 14)
- Use null-conditional assignment (`customer?.Order = value;`) (C# 14)

## Architecture

- **Modular monolith** structure with clear module boundaries
- **Vertical slice architecture** within modules
- Clean separation between modules (no direct cross-module references)
- Shared kernel for common abstractions

## Pull Requests

- Use descriptive PR titles following conventional commits format
- Include a summary of changes in the description
- Reference related issues: `Closes #issue_number`
- Ensure CI passes before requesting review
- Keep PRs focused and small (prefer multiple small PRs over one large PR)

## Testing

- Write tests for new features and bug fixes
- Follow Arrange-Act-Assert pattern
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Use **xUnit v3** (`xunit.v3` package) for test framework
- Use **Shouldly** for assertions (not FluentAssertions due to v8+ licensing)
- Example assertion: `result.ShouldBe(expected);`

## Security

- Never commit secrets, API keys, or connection strings
- Use environment variables or Azure Key Vault for sensitive data
- Follow OWASP security guidelines
