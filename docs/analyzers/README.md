# Medley Analyzers

Roslyn analyzers for enforcing Medley architecture patterns and module boundaries.

## Overview

The `Melodic.SharedKernel.Analyzers` package provides compile-time enforcement of:

- **Module boundaries** - Ensures modules only communicate via Contracts
- **Layer dependencies** - Enforces Clean Architecture layering rules
- **Naming conventions** - Repository, Handler, Validator suffixes
- **CQRS patterns** - Command/Query separation, Result pattern usage
- **DDD patterns** - Aggregate encapsulation, value object design
- **Async best practices** - CancellationToken propagation, async void detection

## Installation

```xml
<PackageReference Include="Melodic.SharedKernel.Analyzers" Version="0.1.0" />
```

The package is a development dependency and includes no runtime assemblies.

## Diagnostic ID Format

All diagnostics follow the pattern: `{PREFIX}{CATEGORY}{NUMBER}`

```
MDYARCH001
│││  ││  │
│││  ││  └─ Sequential number within category (001-999)
│││  └└──── Category code (ARCH, NAME, CQRS, DDD, RES, ASYNC)
└└└──────── Prefix (configurable, default: MDY)
```

### Why This Format?

- **Prefix** - Required for NuGet package uniqueness (avoids collision with CA, CS, IDE rules)
- **Category** - Enables independent numbering per category (no insertion collisions)
- **Number** - Sequential within category (3 digits = 999 rules per category)

## Categories

| Category | Code | Full Prefix | Description |
|----------|------|-------------|-------------|
| Architecture | `ARCH` | `MDYARCH` | Module boundaries, layer dependencies |
| Naming | `NAME` | `MDYNAME` | Class/interface/method naming conventions |
| CQRS | `CQRS` | `MDYCQRS` | Command/Query pattern violations |
| DomainDrivenDesign | `DDD` | `MDYDDD` | Domain-driven design patterns |
| Result | `RES` | `MDYRES` | Result pattern usage |
| Async | `ASYNC` | `MDYASYNC` | Async/await patterns |

## Rule List

See [rule-registry.md](rule-registry.md) for the complete list of all rules.

### Quick Reference

| Severity | Meaning | Example Rules |
|----------|---------|---------------|
| Error | Must fix before build | Cross-module violations, async void |
| Warning | Should fix, may become error | Missing suffixes, unchecked Result |
| Info | Suggestion, optional | Handler folder location |

## Configuration

### Disable Rules via .editorconfig

```ini
# Disable a specific rule
[*.cs]
dotnet_diagnostic.MDYARCH001.severity = none

# Change severity
dotnet_diagnostic.MDYNAME001.severity = suggestion

# Disable layer rules for vertical slice modules
[src/Modules/SimpleModule/**/*.cs]
dotnet_diagnostic.MDYARCH003.severity = none
dotnet_diagnostic.MDYARCH004.severity = none
dotnet_diagnostic.MDYARCH005.severity = none
```

### Opt-Out via Assembly Attribute

Modules can opt-out of layer enforcement:

```csharp
// In module's AssemblyInfo.cs
[assembly: ModuleArchitecture(ModuleStyle.VerticalSlices)]
```

## Module Flexibility

Medley supports multiple architectural styles within modules. See [Module Patterns - Architectural Flexibility](../architecture/module-patterns.md#module-architectural-flexibility) for complete documentation on supported styles and opt-out mechanisms.

## Code Fixes

Many rules include automatic code fixes:

| Rule | Fix Action |
|------|------------|
| MDYNAME001-007 | Add missing suffix to class name |
| MDYDDD001 | Wrap collection with `AsReadOnly()` |
| MDYDDD002 | Change public setter to private |
| MDYASYNC001 | Add `CancellationToken` parameter |

### Smart Suffix Replacement (MDYNAME)

The naming rule code fixes intelligently handle partial suffix matches:

| Current Name | Applied Fix | Notes |
|--------------|-------------|-------|
| `UserConfig` | `UserConfiguration` | Replaces "Config" with full suffix |
| `OrderSpec` | `OrderSpecification` | Replaces "Spec" with full suffix |
| `UserRepo` | `UserRepository` | Replaces "Repo" with full suffix |
| `PaymentSvc` | `PaymentService` | Replaces "Svc" with full suffix |
| `UserDTO` | `UserDto` | Normalizes case |
| `UserStore` | `UserStoreRepository` | Appends when no partial match |

This prevents awkward double-suffix names like `UserConfigConfiguration`.

## Documentation

- [Rule Registry](rule-registry.md) - Complete list of all rules
- Individual rule docs: `rules/MDYXXXX.md`

## Development

### Building the Analyzer

```bash
dotnet build src/SharedKernel/SharedKernel.Analyzers
```

### Running Tests

```bash
dotnet test tests/SharedKernel/SharedKernel.Analyzers.Tests
```

### Creating New Rules

See the implementation guidelines in the analyzer project for patterns on:
- Analyzer registration
- DiagnosticDescriptor creation
- Code fix providers
- Testing with xUnit v3 + Shouldly
