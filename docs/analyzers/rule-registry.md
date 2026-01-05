# Analyzer Rule Registry

Authoritative index of all Medley analyzer rules. Click rule IDs for detailed documentation.

---

## Architecture Rules (MDYARCH)

Module boundaries and layer dependency enforcement.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYARCH001`](rules/MDYARCH001.md) | CrossModuleDirectReference | Error | No | Module directly references another module's non-Contracts project |
| [`MDYARCH002`](rules/MDYARCH002.md) | CrossModuleDatabaseAccess | Error | No | Module accesses another module's database schema directly |
| [`MDYARCH003`](rules/MDYARCH003.md) | DomainReferencesApplication | Error | No | Domain layer references Application layer |
| [`MDYARCH004`](rules/MDYARCH004.md) | DomainReferencesInfrastructure | Error | No | Domain layer references Infrastructure layer |
| [`MDYARCH005`](rules/MDYARCH005.md) | ApplicationReferencesInfrastructure | Error | No | Application layer references Infrastructure layer |
| [`MDYARCH006`](rules/MDYARCH006.md) | ContractsReferencesInternal | Error | No | Contracts project references non-Contracts project |

---

## Naming Rules (MDYNAME)

Class and interface naming conventions.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYNAME001`](rules/MDYNAME001.md) | RepositoryMissingSuffix | Warning | Yes | Repository implementation missing `Repository` suffix |
| [`MDYNAME002`](rules/MDYNAME002.md) | ValidatorMissingSuffix | Warning | Yes | FluentValidation validator missing `Validator` suffix |
| [`MDYNAME003`](rules/MDYNAME003.md) | HandlerMissingSuffix | Warning | Yes | MediatR handler missing `Handler` suffix |
| [`MDYNAME004`](rules/MDYNAME004.md) | SpecificationMissingSuffix | Warning | Yes | Specification class missing `Specification` suffix |
| [`MDYNAME005`](rules/MDYNAME005.md) | ServiceMissingSuffix | Info | Yes | Domain/Application service missing `Service` suffix |
| [`MDYNAME006`](rules/MDYNAME006.md) | ConfigurationMissingSuffix | Warning | Yes | EF Core config missing `Configuration` suffix |
| [`MDYNAME007`](rules/MDYNAME007.md) | DtoMissingSuffix | Info | Yes | DTO class missing `Dto` suffix |

> **Smart Code Fix:** The code fix provider intelligently replaces partial suffixes (e.g., `Config` → `Configuration`, `Spec` → `Specification`, `Repo` → `Repository`) instead of appending, preventing awkward names like `UserConfigConfiguration`.

---

## CQRS Rules (MDYCQRS)

Command/Query separation patterns.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYCQRS001`](rules/MDYCQRS001.md) | CommandNotReturningResult | Warning | No | Command should return `Result<T>` not raw type |
| [`MDYCQRS002`](rules/MDYCQRS002.md) | QueryWithSideEffects | Error | No | Query handler modifies state (calls Add/Update/Delete) |
| [`MDYCQRS003`](rules/MDYCQRS003.md) | MultipleHandlersForRequest | Error | No | Multiple handlers registered for same request |
| [`MDYCQRS004`](rules/MDYCQRS004.md) | HandlerNotInFeaturesFolder | Info | No | Handler not located in Features/{FeatureName}/ folder |
| [`MDYCQRS005`](rules/MDYCQRS005.md) | ValidatorNotWithCommand | Info | No | Validator not in same folder as its command |

---

## DDD Rules (MDYDDD)

Domain-Driven Design patterns.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYDDD001`](rules/MDYDDD001.md) | MutableCollectionExposed | Error | Yes | Aggregate exposes mutable collection (use `AsReadOnly()`) |
| [`MDYDDD002`](rules/MDYDDD002.md) | PublicSetterOnEntity | Warning | Yes | Entity has public setter (should use methods) |
| [`MDYDDD003`](rules/MDYDDD003.md) | DomainLogicInHandler | Warning | No | Complex business logic in handler (move to domain) |
| [`MDYDDD004`](rules/MDYDDD004.md) | AggregateReferenceByObject | Warning | No | Aggregate references another aggregate by ID, not object |
| [`MDYDDD005`](rules/MDYDDD005.md) | ValueObjectWithIdentity | Error | No | Value object has Id property (should be record) |

---

## Result Pattern Rules (MDYRES)

Explicit error handling patterns.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYRES001`](rules/MDYRES001.md) | ThrowingForBusinessFailure | Warning | No | Throwing exception for expected business failure |
| [`MDYRES002`](rules/MDYRES002.md) | IgnoringResultValue | Warning | No | Result value not checked before accessing `.Value` |
| [`MDYRES003`](rules/MDYRES003.md) | ResultNotAwaited | Error | No | Async Result not awaited |

---

## Async Rules (MDYASYNC)

Async/await best practices.

| ID | Name | Severity | Fixable | Description |
|----|------|----------|---------|-------------|
| [`MDYASYNC001`](rules/MDYASYNC001.md) | MissingCancellationToken | Warning | Yes | Async method missing CancellationToken parameter |
| [`MDYASYNC002`](rules/MDYASYNC002.md) | CancellationTokenNotPassed | Warning | No | CancellationToken not passed to downstream async call |
| [`MDYASYNC003`](rules/MDYASYNC003.md) | AsyncVoidMethod | Error | No | Async void method (except event handlers) |

---

## Suppressing Diagnostics

### Inline Suppression

```csharp
#pragma warning disable MDYARCH001
using Users.Domain.Entities;  // Intentional cross-module reference
#pragma warning restore MDYARCH001
```

### Attribute Suppression

```csharp
[SuppressMessage("Medley", "MDYARCH001", Justification = "Legacy code migration")]
public class LegacyService { }
```

### .editorconfig Suppression

```ini
[src/LegacyCode/**/*.cs]
dotnet_diagnostic.MDYARCH001.severity = none
```
