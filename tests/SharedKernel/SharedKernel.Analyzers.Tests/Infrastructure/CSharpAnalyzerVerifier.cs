using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SharedKernel.Analyzers.Tests.Infrastructure;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, ShouldlyVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, ShouldlyVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, ShouldlyVerifier>.Diagnostic(descriptor);

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    public static Task VerifyAnalyzerAsync(string source)
        => VerifyAnalyzerAsync(source, []);

    public class Test : CSharpAnalyzerTest<TAnalyzer, ShouldlyVerifier>
    {
        public Test() => ReferenceAssemblies = VerifierConfiguration.ReferenceAssemblies;

        protected override ParseOptions CreateParseOptions() =>
            VerifierConfiguration.CreateParseOptions();
    }
}
