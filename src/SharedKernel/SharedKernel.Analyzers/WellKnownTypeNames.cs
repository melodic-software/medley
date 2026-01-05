namespace SharedKernel.Analyzers;

/// <summary>
/// Constants for well-known type names used by analyzers.
/// </summary>
/// <remarks>
/// Using constants instead of magic strings provides:
/// - Compile-time validation (typos caught at build time)
/// - Easier refactoring and maintenance
/// - IntelliSense support
/// - Single source of truth (SSOT)
/// </remarks>
public static class WellKnownTypeNames
{
    // MediatR Types

    /// <summary>
    /// MediatR IRequestHandler interface name.
    /// </summary>
    public const string IRequestHandler = "IRequestHandler";

    /// <summary>
    /// MediatR INotificationHandler interface name.
    /// </summary>
    public const string INotificationHandler = "INotificationHandler";



    // FluentValidation Types

    /// <summary>
    /// FluentValidation AbstractValidator base class name.
    /// </summary>
    public const string AbstractValidator = "AbstractValidator";

    /// <summary>
    /// Full metadata name for FluentValidation AbstractValidator.
    /// </summary>
    public const string AbstractValidatorMetadataName = "FluentValidation.AbstractValidator`1";



    // EF Core Types

    /// <summary>
    /// EF Core IEntityTypeConfiguration interface name.
    /// </summary>
    public const string IEntityTypeConfiguration = "IEntityTypeConfiguration";

    /// <summary>
    /// Full metadata name for EF Core IEntityTypeConfiguration.
    /// </summary>
    public const string IEntityTypeConfigurationMetadataName =
        "Microsoft.EntityFrameworkCore.IEntityTypeConfiguration`1";



    // Specification Pattern Types

    /// <summary>
    /// Specification base class name.
    /// </summary>
    public const string Specification = "Specification";

    /// <summary>
    /// ISpecification interface prefix for pattern matching.
    /// </summary>
    public const string ISpecificationPrefix = "ISpecification";



    // Repository Pattern Types

    /// <summary>
    /// IRepository interface prefix for pattern matching.
    /// </summary>
    public const string IRepositoryPrefix = "IRepository";

    /// <summary>
    /// Repository suffix for interface pattern matching.
    /// </summary>
    public const string RepositorySuffix = "Repository";



    // Result Pattern Types

    /// <summary>
    /// Result type name.
    /// </summary>
    public const string Result = "Result";



    // Domain-Driven Design Types

    /// <summary>
    /// Entity base class name.
    /// </summary>
    public const string Entity = "Entity";

    /// <summary>
    /// AggregateRoot base class name.
    /// </summary>
    public const string AggregateRoot = "AggregateRoot";

    /// <summary>
    /// ValueObject base class name.
    /// </summary>
    public const string ValueObject = "ValueObject";

    /// <summary>
    /// IDomainEvent interface name.
    /// </summary>
    public const string IDomainEvent = "IDomainEvent";


}
