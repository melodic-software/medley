using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects specification classes that don't end with 'Specification' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME004
/// Severity: Warning
///
/// Specification pattern classes should end with 'Specification' suffix.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpecificationSuffixAnalyzer : SuffixAnalyzerBase
{
    protected override string RequiredSuffix => "Specification";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME004_SpecificationMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check if inherits from Specification<T> or implements ISpecification<T>
        // Note: Guard clauses (abstract/interface) handled by base class
        return InheritsFrom(typeSymbol, WellKnownTypeNames.Specification) ||
               ImplementsInterfaceWithPattern(typeSymbol, startsWith: WellKnownTypeNames.ISpecificationPrefix);
    }
}
