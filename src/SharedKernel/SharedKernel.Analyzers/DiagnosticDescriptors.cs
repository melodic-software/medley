using Microsoft.CodeAnalysis;

namespace SharedKernel.Analyzers;

/// <summary>
/// Central repository for all DiagnosticDescriptor definitions.
/// </summary>
/// <remarks>
/// Each descriptor follows the pattern: {Category}_{RuleName}
/// All help links point to: https://github.com/melodic-software/medley/blob/main/docs/analyzers/rules/{RuleId}.md
/// </remarks>
public static class DiagnosticDescriptors
{
    private const string HelpLinkBase = "https://github.com/melodic-software/medley/blob/main/docs/analyzers/rules/";

    // Factory Methods

    private static DiagnosticDescriptor CreateDescriptor(
        string id,
        string title,
        string messageFormat,
        string category,
        DiagnosticSeverity severity,
        string description) =>
        new(id, title, messageFormat, category, severity, isEnabledByDefault: true,
            description, HelpLinkBase + id + ".md");

    private static DiagnosticDescriptor CreateArchitectureError(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Architecture,
            DiagnosticSeverity.Error, description);

    private static DiagnosticDescriptor CreateNamingWarning(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Naming,
            DiagnosticSeverity.Warning, description);

    private static DiagnosticDescriptor CreateNamingInfo(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Naming,
            DiagnosticSeverity.Info, description);

    private static DiagnosticDescriptor CreateCqrsError(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Cqrs,
            DiagnosticSeverity.Error, description);

    private static DiagnosticDescriptor CreateCqrsWarning(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Cqrs,
            DiagnosticSeverity.Warning, description);

    private static DiagnosticDescriptor CreateCqrsInfo(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Cqrs,
            DiagnosticSeverity.Info, description);

    private static DiagnosticDescriptor CreateDddError(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.DomainDrivenDesign,
            DiagnosticSeverity.Error, description);

    private static DiagnosticDescriptor CreateDddWarning(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.DomainDrivenDesign,
            DiagnosticSeverity.Warning, description);

    private static DiagnosticDescriptor CreateResultError(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Result,
            DiagnosticSeverity.Error, description);

    private static DiagnosticDescriptor CreateResultWarning(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Result,
            DiagnosticSeverity.Warning, description);

    private static DiagnosticDescriptor CreateAsyncError(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Async,
            DiagnosticSeverity.Error, description);

    private static DiagnosticDescriptor CreateAsyncWarning(
        string id, string title, string messageFormat, string description) =>
        CreateDescriptor(id, title, messageFormat, DiagnosticCategories.Async,
            DiagnosticSeverity.Warning, description);



    // Architecture Rules (MDYARCH)

    public static readonly DiagnosticDescriptor MDYARCH001_CrossModuleDirectReference =
        CreateArchitectureError(DiagnosticIds.MDYARCH001,
            "Cross-module direct reference detected",
            "Module '{0}' should not directly reference '{1}'",
            "Modules must communicate through their Contracts projects only. Direct references between modules violate modular monolith boundaries.");

    public static readonly DiagnosticDescriptor MDYARCH002_CrossModuleDatabaseAccess =
        CreateArchitectureError(DiagnosticIds.MDYARCH002,
            "Cross-module database access detected",
            "Module '{0}' should not access database schema '{1}' owned by another module",
            "Each module owns its database schema. Cross-module database access bypasses the intended communication layer.");

    public static readonly DiagnosticDescriptor MDYARCH003_DomainReferencesApplication =
        CreateArchitectureError(DiagnosticIds.MDYARCH003,
            "Domain layer references Application layer",
            "Domain layer should not reference Application layer",
            "Clean Architecture requires Domain to have no dependencies on outer layers.");

    public static readonly DiagnosticDescriptor MDYARCH004_DomainReferencesInfrastructure =
        CreateArchitectureError(DiagnosticIds.MDYARCH004,
            "Domain layer references Infrastructure layer",
            "Domain layer should not reference Infrastructure layer",
            "Clean Architecture requires Domain to have no dependencies on outer layers.");

    public static readonly DiagnosticDescriptor MDYARCH005_ApplicationReferencesInfrastructure =
        CreateArchitectureError(DiagnosticIds.MDYARCH005,
            "Application layer references Infrastructure layer",
            "Application layer should not reference Infrastructure layer",
            "Clean Architecture requires Application to depend only on Domain, not Infrastructure.");

    public static readonly DiagnosticDescriptor MDYARCH006_ContractsReferencesInternal =
        CreateArchitectureError(DiagnosticIds.MDYARCH006,
            "Contracts project references internal project",
            "Contracts project should not reference '{0}'",
            "Contracts projects expose the public API of a module and must not reference internal implementation details.");



    // Naming Rules (MDYNAME)

    public static readonly DiagnosticDescriptor MDYNAME001_RepositoryMissingSuffix =
        CreateNamingWarning(DiagnosticIds.MDYNAME001,
            "Repository missing suffix",
            "Class '{0}' implements IRepository but doesn't end with 'Repository'",
            "Repository implementations should end with 'Repository' suffix for consistency.");

    public static readonly DiagnosticDescriptor MDYNAME002_ValidatorMissingSuffix =
        CreateNamingWarning(DiagnosticIds.MDYNAME002,
            "Validator missing suffix",
            "Class '{0}' extends AbstractValidator but doesn't end with 'Validator'",
            "FluentValidation validators should end with 'Validator' suffix for consistency.");

    public static readonly DiagnosticDescriptor MDYNAME003_HandlerMissingSuffix =
        CreateNamingWarning(DiagnosticIds.MDYNAME003,
            "Handler missing suffix",
            "Class '{0}' implements IRequestHandler but doesn't end with 'Handler'",
            "MediatR handlers should end with 'Handler' suffix. Namespace provides Command/Query context.");

    public static readonly DiagnosticDescriptor MDYNAME004_SpecificationMissingSuffix =
        CreateNamingWarning(DiagnosticIds.MDYNAME004,
            "Specification missing suffix",
            "Class '{0}' extends Specification but doesn't end with 'Specification'",
            "Specification pattern classes should end with 'Specification' suffix.");

    public static readonly DiagnosticDescriptor MDYNAME005_ServiceMissingSuffix =
        CreateNamingInfo(DiagnosticIds.MDYNAME005,
            "Service missing suffix",
            "Class '{0}' appears to be a service but doesn't end with 'Service'",
            "Domain and Application services should end with 'Service' suffix.");

    public static readonly DiagnosticDescriptor MDYNAME006_ConfigurationMissingSuffix =
        CreateNamingWarning(DiagnosticIds.MDYNAME006,
            "Configuration missing suffix",
            "Class '{0}' implements IEntityTypeConfiguration but doesn't end with 'Configuration'",
            "EF Core entity configurations should end with 'Configuration' suffix.");

    public static readonly DiagnosticDescriptor MDYNAME007_DtoMissingSuffix =
        CreateNamingInfo(DiagnosticIds.MDYNAME007,
            "DTO missing suffix",
            "Class '{0}' appears to be a DTO but doesn't end with 'Dto'",
            "Data Transfer Objects should end with 'Dto' suffix.");



    // CQRS Rules (MDYCQRS)

    public static readonly DiagnosticDescriptor MDYCQRS001_CommandNotReturningResult =
        CreateCqrsWarning(DiagnosticIds.MDYCQRS001,
            "Command not returning Result",
            "Command '{0}' should return Result<T> for explicit error handling",
            "Commands should return Result<T> for explicit error handling instead of throwing exceptions.");

    public static readonly DiagnosticDescriptor MDYCQRS002_QueryWithSideEffects =
        CreateCqrsError(DiagnosticIds.MDYCQRS002,
            "Query has side effects",
            "Query handler '{0}' modifies state",
            "Query handlers must be read-only. State modifications belong in command handlers.");

    public static readonly DiagnosticDescriptor MDYCQRS003_MultipleHandlersForRequest =
        CreateCqrsError(DiagnosticIds.MDYCQRS003,
            "Multiple handlers for request",
            "Request '{0}' has multiple handlers",
            "Each command/query should have exactly one handler.");

    public static readonly DiagnosticDescriptor MDYCQRS004_HandlerNotInFeaturesFolder =
        CreateCqrsInfo(DiagnosticIds.MDYCQRS004,
            "Handler not in Features folder",
            "Handler '{0}' should be in Features/{FeatureName}/ folder",
            "Vertical slice organization: handlers should be in Features/{FeatureName}/ folder.");

    public static readonly DiagnosticDescriptor MDYCQRS005_ValidatorNotWithCommand =
        CreateCqrsInfo(DiagnosticIds.MDYCQRS005,
            "Validator not with command",
            "Validator '{0}' should be in the same folder as its command",
            "Validators should be co-located with their commands for discoverability.");



    // DDD Rules (MDYDDD)

    public static readonly DiagnosticDescriptor MDYDDD001_MutableCollectionExposed =
        CreateDddError(DiagnosticIds.MDYDDD001,
            "Mutable collection exposed",
            "Aggregate '{0}' exposes mutable collection '{1}'",
            "Aggregates must not expose mutable collections. Use AsReadOnly() or IReadOnlyCollection<T>.");

    public static readonly DiagnosticDescriptor MDYDDD002_PublicSetterOnEntity =
        CreateDddWarning(DiagnosticIds.MDYDDD002,
            "Public setter on entity",
            "Entity '{0}' has public setter for '{1}'",
            "Entities should encapsulate state changes via methods, not public setters.");

    public static readonly DiagnosticDescriptor MDYDDD003_DomainLogicInHandler =
        CreateDddWarning(DiagnosticIds.MDYDDD003,
            "Domain logic in handler",
            "Handler '{0}' contains business logic that should be in the domain",
            "Complex business logic should live in the domain layer, not handlers.");

    public static readonly DiagnosticDescriptor MDYDDD004_AggregateReferenceByObject =
        CreateDddWarning(DiagnosticIds.MDYDDD004,
            "Aggregate references by object",
            "Aggregate '{0}' references aggregate '{1}' by object",
            "Aggregates should reference other aggregates by ID, not object reference.");

    public static readonly DiagnosticDescriptor MDYDDD005_ValueObjectWithIdentity =
        CreateDddError(DiagnosticIds.MDYDDD005,
            "Value object with identity",
            "Value object '{0}' has Id property",
            "Value objects are defined by their attributes, not identity. Use records.");



    // Result Pattern Rules (MDYRES)

    public static readonly DiagnosticDescriptor MDYRES001_ThrowingForBusinessFailure =
        CreateResultWarning(DiagnosticIds.MDYRES001,
            "Throwing for business failure",
            "Throwing exception for expected business failure",
            "Use Result<T>.Failure() for expected business failures, not exceptions.");

    public static readonly DiagnosticDescriptor MDYRES002_IgnoringResultValue =
        CreateResultWarning(DiagnosticIds.MDYRES002,
            "Result value not checked",
            "Accessing Result.Value without checking IsSuccess first",
            "Always check IsSuccess before accessing Result.Value to avoid runtime errors.");

    public static readonly DiagnosticDescriptor MDYRES003_ResultNotAwaited =
        CreateResultError(DiagnosticIds.MDYRES003,
            "Async Result not awaited",
            "Async Result operation not awaited",
            "Async Result operations must be awaited.");



    // Async Rules (MDYASYNC)

    public static readonly DiagnosticDescriptor MDYASYNC001_MissingCancellationToken =
        CreateAsyncWarning(DiagnosticIds.MDYASYNC001,
            "Missing CancellationToken",
            "Async method '{0}' should accept CancellationToken parameter",
            "Async methods should accept CancellationToken to support cancellation.");

    public static readonly DiagnosticDescriptor MDYASYNC002_CancellationTokenNotPassed =
        CreateAsyncWarning(DiagnosticIds.MDYASYNC002,
            "CancellationToken not passed",
            "CancellationToken not passed to async call '{0}'",
            "Pass CancellationToken to downstream async calls to propagate cancellation.");

    public static readonly DiagnosticDescriptor MDYASYNC003_AsyncVoidMethod =
        CreateAsyncError(DiagnosticIds.MDYASYNC003,
            "Async void method",
            "Async void method '{0}'",
            "Async void methods cannot be awaited and exceptions are unobservable. Use async Task.");


}
