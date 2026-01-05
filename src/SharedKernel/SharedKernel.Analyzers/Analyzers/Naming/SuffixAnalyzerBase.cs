using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Naming;

/// <summary>
/// Base class for analyzers that check for required suffixes on type names.
/// </summary>
/// <remarks>
/// Derived classes only need to implement:
/// - <see cref="RequiredSuffix"/> - the suffix to enforce
/// - <see cref="Descriptor"/> - the diagnostic descriptor
/// - <see cref="ShouldHaveSuffix"/> - the matching logic (without guard clauses)
///
/// Guard clauses for interfaces and abstract classes are handled in the base class.
/// </remarks>
public abstract class SuffixAnalyzerBase : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the required suffix for types matching this analyzer's criteria.
    /// </summary>
    protected abstract string RequiredSuffix { get; }

    /// <summary>
    /// Gets the diagnostic descriptor for this analyzer.
    /// </summary>
    protected abstract DiagnosticDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the string comparison to use when checking for existing suffix.
    /// Override to <see cref="StringComparison.OrdinalIgnoreCase"/> for case-insensitive matching.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="StringComparison.Ordinal"/> for case-sensitive matching.
    /// The DtoSuffixAnalyzer overrides this to handle both "Dto" and "DTO" variants.
    /// </remarks>
    protected virtual StringComparison SuffixComparison => StringComparison.Ordinal;

    /// <summary>
    /// Gets additional suffix variants to accept (e.g., "DTO" as alternative to "Dto").
    /// </summary>
    /// <remarks>
    /// Default is empty. Override to provide case variants or alternative spellings.
    /// </remarks>
    protected virtual ImmutableArray<string> AlternateSuffixes => [];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    /// <summary>
    /// Property key for the required suffix passed to code fixes.
    /// </summary>
    public const string RequiredSuffixProperty = "RequiredSuffix";

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Common guard: Skip interfaces and abstract classes (DRY - moved from derived classes)
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return;
        }

        // Skip if already has the required suffix
        if (HasRequiredSuffix(namedTypeSymbol.Name))
        {
            return;
        }

        // Check if this type matches the criteria for requiring the suffix
        if (!ShouldHaveSuffix(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Build properties dictionary to pass suffix to code fix
        ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty
            .Add(RequiredSuffixProperty, RequiredSuffix);

        // Report diagnostic with properties
        var diagnostic = Diagnostic.Create(
            Descriptor,
            namedTypeSymbol.Locations[0],
            properties,
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Checks if the type name already has the required suffix (including alternates).
    /// </summary>
    private bool HasRequiredSuffix(string typeName)
    {
        if (typeName.EndsWith(RequiredSuffix, SuffixComparison))
        {
            return true;
        }

        foreach (string alternate in AlternateSuffixes)
        {
            if (typeName.EndsWith(alternate, SuffixComparison))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given type should have the required suffix.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check (already filtered: not abstract, not interface).</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>True if the type should have the required suffix.</returns>
    /// <remarks>
    /// Implementers do NOT need to check for abstract classes or interfaces - this is handled by the base class.
    /// Focus only on the type-specific matching logic (e.g., implements IRequestHandler, inherits from AbstractValidator).
    /// </remarks>
    protected abstract bool ShouldHaveSuffix(INamedTypeSymbol typeSymbol, Compilation compilation);

    /// <summary>
    /// Checks if the type implements an interface with the specified name (exact or generic match).
    /// </summary>
    /// <remarks>
    /// Performance: Uses direct name comparison instead of ToDisplayString() to avoid string allocations.
    /// </remarks>
    protected static bool ImplementsInterface(INamedTypeSymbol typeSymbol, string interfaceName) =>
        typeSymbol.AllInterfaces.Any(i =>
            i.Name == interfaceName ||
            i.OriginalDefinition.Name == interfaceName);

    /// <summary>
    /// Checks if the type implements an interface matching a naming pattern.
    /// </summary>
    protected static bool ImplementsInterfaceWithPattern(
        INamedTypeSymbol typeSymbol,
        string? startsWith = null,
        string? endsWith = null) =>
        typeSymbol.AllInterfaces.Any(i =>
            (startsWith is null || i.Name.StartsWith(startsWith, StringComparison.Ordinal)) &&
            (endsWith is null || i.Name.EndsWith(endsWith, StringComparison.Ordinal)));

    /// <summary>
    /// Checks if the type or any of its base types inherits from a class with the specified name.
    /// </summary>
    /// <remarks>
    /// Performance: Uses OriginalDefinition.Name instead of ToDisplayString().Contains() to avoid
    /// string allocations in hot paths. This is critical for analyzers running on large codebases.
    /// </remarks>
    protected static bool InheritsFrom(INamedTypeSymbol typeSymbol, string baseTypeName)
    {
        INamedTypeSymbol? current = typeSymbol.BaseType;
        while (current is not null)
        {
            // Check both Name and OriginalDefinition.Name for generic types
            if (current.Name == baseTypeName ||
                current.OriginalDefinition.Name == baseTypeName)
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Checks if the type inherits from a well-known type by its full metadata name.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="metadataName">The full metadata name (e.g., "FluentValidation.AbstractValidator`1").</param>
    /// <returns>True if the type inherits from the specified base type.</returns>
    /// <remarks>
    /// This is the preferred method for checking well-known types as it uses symbol comparison
    /// instead of string matching, which is both more precise and more performant.
    /// </remarks>
    protected static bool InheritsFromWellKnownType(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        string metadataName)
    {
        INamedTypeSymbol? wellKnownType = compilation.GetTypeByMetadataName(metadataName);
        if (wellKnownType is null)
        {
            return false;
        }

        INamedTypeSymbol? current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, wellKnownType))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Checks if the type implements a well-known interface by its full metadata name.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="metadataName">The full metadata name (e.g., "MediatR.IRequestHandler`2").</param>
    /// <returns>True if the type implements the specified interface.</returns>
    /// <remarks>
    /// This is the preferred method for checking well-known interfaces as it uses symbol comparison
    /// instead of string matching, which is both more precise and more performant.
    /// </remarks>
    protected static bool ImplementsWellKnownInterface(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        string metadataName)
    {
        INamedTypeSymbol? wellKnownInterface = compilation.GetTypeByMetadataName(metadataName);
        return wellKnownInterface is not null && typeSymbol.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, wellKnownInterface));
    }
}
