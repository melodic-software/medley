using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects repository implementations that don't end with 'Repository' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME001
/// Severity: Warning
///
/// Repository implementations should end with 'Repository' suffix for
/// consistency and discoverability.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RepositorySuffixAnalyzer : SuffixAnalyzerBase
{
    protected override string RequiredSuffix => "Repository";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME001_RepositoryMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check if implements IRepository<T> or any interface ending with Repository
        // Note: Guard clauses (abstract/interface) handled by base class
        return ImplementsInterfaceWithPattern(typeSymbol, startsWith: WellKnownTypeNames.IRepositoryPrefix) ||
               ImplementsInterfaceWithPattern(typeSymbol, endsWith: WellKnownTypeNames.RepositorySuffix);
    }
}
