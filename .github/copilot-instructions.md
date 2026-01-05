<!-- Last reviewed: 2026-01-04 -->

# Copilot Instructions for Medley

## Project Overview

Medley is a .NET 10 modular monolith application using C# 14, following trunk-based development with squash merging.

## Commit Convention (REQUIRED)

All commits **must** follow [Conventional Commits](https://www.conventionalcommits.org/) format.

See [CONTRIBUTING.md](../CONTRIBUTING.md#commit-messages-conventional-commits) for the full format, types, and examples.

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
