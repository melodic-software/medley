namespace SharedKernel.Analyzers;

/// <summary>
/// Categories for organizing diagnostics in the analyzer output.
/// </summary>
/// <remarks>
/// Categories appear in IDE error lists and can be used for filtering.
/// Format: Medley.{CategoryName}
/// </remarks>
public static class DiagnosticCategories
{
    /// <summary>Module boundaries and cross-module violations.</summary>
    public const string Architecture = "Medley.Architecture";

    /// <summary>Class/interface naming conventions (suffixes).</summary>
    public const string Naming = "Medley.Naming";

    /// <summary>Command/Query separation patterns.</summary>
    public const string Cqrs = "Medley.CQRS";

    /// <summary>Domain-Driven Design patterns.</summary>
    public const string DomainDrivenDesign = "Medley.DDD";

    /// <summary>Result pattern for explicit error handling.</summary>
    public const string Result = "Medley.Result";

    /// <summary>Async/await best practices.</summary>
    public const string Async = "Medley.Async";
}
