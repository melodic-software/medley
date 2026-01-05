using System.Collections.Immutable;

namespace SharedKernel.Analyzers;

/// <summary>
/// Constants for architectural layer names used by analyzers.
/// </summary>
/// <remarks>
/// Using constants instead of magic strings provides:
/// - Compile-time validation (typos caught at build time)
/// - Easier refactoring and maintenance
/// - IntelliSense support
/// - Single source of truth (SSOT)
/// </remarks>
public static class LayerNames
{
    /// <summary>
    /// Domain layer - contains entities, value objects, and domain logic.
    /// </summary>
    public const string Domain = "Domain";

    /// <summary>
    /// Application layer - contains use cases, commands, queries, and handlers.
    /// </summary>
    public const string Application = "Application";

    /// <summary>
    /// Infrastructure layer - contains data access, external services, and implementations.
    /// </summary>
    public const string Infrastructure = "Infrastructure";

    /// <summary>
    /// Contracts layer - contains DTOs, events, and integration contracts.
    /// The only layer that may be referenced across module boundaries.
    /// </summary>
    public const string Contracts = "Contracts";

    /// <summary>
    /// Internal layers that should not be referenced across module boundaries.
    /// Only Contracts layer is allowed for cross-module communication.
    /// </summary>
    public static readonly ImmutableHashSet<string> InternalLayers =
        [Domain, Application, Infrastructure];
}

/// <summary>
/// Namespace segment patterns used for layer detection in type namespaces.
/// </summary>
/// <remarks>
/// These patterns include the leading dot to ensure accurate matching
/// (e.g., ".Domain" matches "Orders.Domain" but not "SomeDomainHelper").
/// </remarks>
public static class NamespaceSegments
{
    /// <summary>
    /// Domain layer namespace segment.
    /// </summary>
    public const string Domain = ".Domain";

    /// <summary>
    /// Application layer namespace segment.
    /// </summary>
    public const string Application = ".Application";

    /// <summary>
    /// Infrastructure layer namespace segment.
    /// </summary>
    public const string Infrastructure = ".Infrastructure";

    /// <summary>
    /// Contracts layer namespace segment.
    /// </summary>
    public const string Contracts = ".Contracts";

    /// <summary>
    /// Services namespace segment (for service classes).
    /// </summary>
    public const string Services = ".Services";
}
