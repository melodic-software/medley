using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SharedKernel.Analyzers.Tests.Infrastructure;

/// <summary>
/// Shared configuration for analyzer and code fix verification tests.
/// </summary>
internal static class VerifierConfiguration
{
    public static ReferenceAssemblies ReferenceAssemblies { get; } = new(
        "net10.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
        Path.Combine("ref", "net10.0"));

    public static ParseOptions CreateParseOptions() =>
        new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(
            Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview);
}
