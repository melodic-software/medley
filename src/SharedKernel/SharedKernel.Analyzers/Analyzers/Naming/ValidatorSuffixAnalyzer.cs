using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects FluentValidation validators that don't end with 'Validator' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME002
/// Severity: Warning
///
/// FluentValidation validators should end with 'Validator' suffix for
/// consistency.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidatorSuffixAnalyzer : SuffixAnalyzerBase
{
    protected override string RequiredSuffix => "Validator";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME002_ValidatorMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check if inherits from AbstractValidator<T>
        // Note: Guard clauses (abstract/interface) handled by base class
        return InheritsFrom(typeSymbol, WellKnownTypeNames.AbstractValidator);
    }
}
