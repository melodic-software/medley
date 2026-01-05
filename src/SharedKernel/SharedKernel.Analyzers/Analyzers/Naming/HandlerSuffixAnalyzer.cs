using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Detects MediatR handlers that don't end with 'Handler' suffix.
/// </summary>
/// <remarks>
/// Rule: MDYNAME003
/// Severity: Warning
///
/// MediatR handlers should end with 'Handler' suffix.
/// Namespace provides Command/Query context.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HandlerSuffixAnalyzer : SuffixAnalyzerBase
{
    protected override string RequiredSuffix => "Handler";

    protected override DiagnosticDescriptor Descriptor =>
        DiagnosticDescriptors.MDYNAME003_HandlerMissingSuffix;

    protected override bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check if implements IRequestHandler<,> or IRequestHandler<>
        // Note: Guard clauses (abstract/interface) handled by base class
        return ImplementsInterface(typeSymbol, WellKnownTypeNames.IRequestHandler);
    }
}
