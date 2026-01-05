using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects EF Core configurations that don't end with 'Configuration' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME006
/// Severity: Warning
///
/// EF Core entity configurations should end with 'Configuration' suffix.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConfigurationSuffixAnalyzer : SuffixAnalyzerBase
{
    protected override string RequiredSuffix => "Configuration";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME006_ConfigurationMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check if implements IEntityTypeConfiguration<T>
        // Note: Guard clauses (abstract/interface) handled by base class
        return ImplementsInterface(typeSymbol, WellKnownTypeNames.IEntityTypeConfiguration);
    }
}
