using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SharedKernel.Analyzers.Tests.Infrastructure;

/// <summary>
/// Base class for suffix analyzer tests providing common test patterns.
/// Inherit from this class to reduce boilerplate in naming analyzer tests.
/// </summary>
/// <typeparam name="TAnalyzer">The analyzer type being tested.</typeparam>
public abstract class SuffixAnalyzerTestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Gets the diagnostic ID produced by this analyzer (e.g., "MDYNAME001").
    /// </summary>
    protected abstract string DiagnosticId { get; }

    /// <summary>
    /// Verifies that the given source code produces no diagnostics.
    /// </summary>
    /// <param name="source">The source code to analyze.</param>
    protected Task VerifyNoDiagnosticAsync(string source)
        => CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source);

    /// <summary>
    /// Verifies that the given source code produces a diagnostic for the specified class name.
    /// Use {|#0:ClassName|} syntax in source to mark the expected diagnostic location.
    /// </summary>
    /// <param name="source">The source code to analyze with marked diagnostic location.</param>
    /// <param name="className">The class name that should be flagged.</param>
    protected async Task VerifyDiagnosticAsync(string source, string className)
    {
        DiagnosticResult expected = CSharpAnalyzerVerifier<TAnalyzer>
            .Diagnostic(DiagnosticId)
            .WithLocation(0)
            .WithArguments(className);

        await CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }

    /// <summary>
    /// Verifies that the given source code produces diagnostics for multiple class names.
    /// Use {|#N:ClassName|} syntax in source to mark each expected diagnostic location.
    /// </summary>
    /// <param name="source">The source code to analyze with marked diagnostic locations.</param>
    /// <param name="classNames">The class names that should be flagged, in order of their markers.</param>
    protected async Task VerifyMultipleDiagnosticsAsync(string source, params string[] classNames)
    {
        var expected = classNames
            .Select((name, index) => CSharpAnalyzerVerifier<TAnalyzer>
                .Diagnostic(DiagnosticId)
                .WithLocation(index)
                .WithArguments(name))
            .ToArray();

        await CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }
}
