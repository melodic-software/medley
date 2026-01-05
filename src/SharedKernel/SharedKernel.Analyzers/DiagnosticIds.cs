namespace SharedKernel.Analyzers;

/// <summary>
/// Constants for analyzer configuration.
/// </summary>
public static class AnalyzerConstants
{
    /// <summary>
    /// Diagnostic ID prefix. Change this to rebrand all rules.
    /// </summary>
    /// <remarks>
    /// Format: {PREFIX}{CATEGORY}{NUMBER}
    /// Example: MDYARCH001, MDYNAME002, MDYCQRS001
    /// </remarks>
    public const string DiagnosticPrefix = "MDY";
}

/// <summary>
/// Diagnostic IDs for SharedKernel analyzers.
/// </summary>
/// <remarks>
/// <para>
/// Format: {PREFIX}{CATEGORY}{NUMBER}
/// - PREFIX: Configurable via <see cref="AnalyzerConstants.DiagnosticPrefix"/> (default: MDY)
/// - CATEGORY: Identifies the rule category (ARCH, NAME, CQRS, DDD, RES, ASYNC)
/// - NUMBER: Sequential within category (001-999)
/// </para>
/// <para>
/// Categories:
/// - ARCH: Architecture (module boundaries, layer dependencies)
/// - NAME: Naming conventions (suffixes)
/// - CQRS: Command/Query patterns
/// - DDD: Domain-Driven Design patterns
/// - RES: Result pattern usage
/// - ASYNC: Async/await patterns
/// </para>
/// <para>
/// See <see cref="DiagnosticDescriptors"/> for full diagnostic definitions.
/// </para>
/// </remarks>
public static class DiagnosticIds
{
    private const string Prefix = AnalyzerConstants.DiagnosticPrefix;

    // Architecture Rules (MDYARCH)
    public const string MDYARCH001 = Prefix + "ARCH001";
    public const string MDYARCH002 = Prefix + "ARCH002";
    public const string MDYARCH003 = Prefix + "ARCH003";
    public const string MDYARCH004 = Prefix + "ARCH004";
    public const string MDYARCH005 = Prefix + "ARCH005";
    public const string MDYARCH006 = Prefix + "ARCH006";

    // Naming Rules (MDYNAME)
    public const string MDYNAME001 = Prefix + "NAME001";
    public const string MDYNAME002 = Prefix + "NAME002";
    public const string MDYNAME003 = Prefix + "NAME003";
    public const string MDYNAME004 = Prefix + "NAME004";
    public const string MDYNAME005 = Prefix + "NAME005";
    public const string MDYNAME006 = Prefix + "NAME006";
    public const string MDYNAME007 = Prefix + "NAME007";

    // CQRS Rules (MDYCQRS)
    public const string MDYCQRS001 = Prefix + "CQRS001";
    public const string MDYCQRS002 = Prefix + "CQRS002";
    public const string MDYCQRS003 = Prefix + "CQRS003";
    public const string MDYCQRS004 = Prefix + "CQRS004";
    public const string MDYCQRS005 = Prefix + "CQRS005";

    // DDD Rules (MDYDDD)
    public const string MDYDDD001 = Prefix + "DDD001";
    public const string MDYDDD002 = Prefix + "DDD002";
    public const string MDYDDD003 = Prefix + "DDD003";
    public const string MDYDDD004 = Prefix + "DDD004";
    public const string MDYDDD005 = Prefix + "DDD005";

    // Result Pattern Rules (MDYRES)
    public const string MDYRES001 = Prefix + "RES001";
    public const string MDYRES002 = Prefix + "RES002";
    public const string MDYRES003 = Prefix + "RES003";

    // Async Rules (MDYASYNC)
    public const string MDYASYNC001 = Prefix + "ASYNC001";
    public const string MDYASYNC002 = Prefix + "ASYNC002";
    public const string MDYASYNC003 = Prefix + "ASYNC003";
}
