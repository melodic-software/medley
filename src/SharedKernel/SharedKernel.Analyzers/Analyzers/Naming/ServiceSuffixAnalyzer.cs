using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects service classes that don't end with 'Service' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME005
/// Severity: Info
///
/// Domain and Application services should end with 'Service' suffix.
/// This rule uses heuristics to detect service-like classes:
/// - Must be in a .Domain, .Application, or .Services namespace
/// - Must have a service-like suffix (Manager, Processor, etc.)
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ServiceSuffixAnalyzer : SuffixAnalyzerBase
{
    /// <summary>
    /// Suffixes that indicate a class is likely a service that should use "Service" suffix.
    /// </summary>
    private static readonly ImmutableArray<string> _serviceLikeSuffixes =
    [
        "Manager",
        "Processor",
        "Coordinator",
        "Provider",
        "Executor",
        "Dispatcher"
    ];

    protected override string RequiredSuffix => "Service";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME005_ServiceMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Note: Guard clauses (abstract/interface) handled by base class
        // Note: Suffix check (already ends with "Service") handled by base class

        // Only flag if in Domain, Application, or Services namespace
        string? containingNamespace = typeSymbol.ContainingNamespace?.ToDisplayString();
        if (containingNamespace is null)
        {
            return false;
        }

        bool isInServiceNamespace = containingNamespace.Contains(NamespaceSegments.Domain) ||
                                    containingNamespace.Contains(NamespaceSegments.Application) ||
                                    containingNamespace.Contains(NamespaceSegments.Services);

        if (!isInServiceNamespace)
        {
            return false;
        }

        // Check if has service-like suffix that should be renamed
        return _serviceLikeSuffixes.Any(suffix =>
            typeSymbol.Name.EndsWith(suffix, StringComparison.Ordinal));
    }
}
