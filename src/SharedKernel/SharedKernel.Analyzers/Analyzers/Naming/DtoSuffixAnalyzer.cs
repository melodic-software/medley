using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects DTO classes that don't end with 'Dto' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME007
/// Severity: Info
///
/// Data Transfer Objects should end with 'Dto' suffix.
/// This rule uses heuristics to detect DTO-like classes:
/// - Must be in a .Contracts namespace
/// - Must have a DTO-like suffix (Response, Request, Model, etc.)
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DtoSuffixAnalyzer : SuffixAnalyzerBase
{
    /// <summary>
    /// Suffixes that indicate a class is likely a DTO.
    /// </summary>
    private static readonly ImmutableArray<string> _dtoLikeSuffixes =
    [
        "Response",
        "Request",
        "Model",
        "Data",
        "Info",
        "Details",
        "Summary",
        "Item",
        "Result"
    ];

    protected override string RequiredSuffix => "Dto";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME007_DtoMissingSuffix;

    /// <summary>
    /// Accept "DTO" as an alternate case variant (some teams prefer uppercase).
    /// </summary>
    protected override ImmutableArray<string> AlternateSuffixes => ["DTO"];

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Only check records and classes (not structs)
        // Note: Guard clauses (abstract/interface) handled by base class
        if (typeSymbol.TypeKind != TypeKind.Class && !typeSymbol.IsRecord)
        {
            return false;
        }

        // Check if in Contracts namespace (strong indicator of DTO)
        string? containingNamespace = typeSymbol.ContainingNamespace?.ToDisplayString();
        bool isInContracts = containingNamespace?.Contains(NamespaceSegments.Contracts) ?? false;

        if (!isInContracts)
        {
            return false;
        }

        // Check if has DTO-like suffix that should be renamed to Dto
        return _dtoLikeSuffixes.Any(suffix =>
            typeSymbol.Name.EndsWith(suffix, StringComparison.Ordinal));
    }
}
