using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using SharedKernel.Analyzers.Analyzers.Naming;

namespace SharedKernel.Analyzers.CodeFixes.Naming;

/// <summary>
/// Provides code fixes for naming suffix diagnostics by renaming types to include the required suffix.
/// </summary>
/// <remarks>
/// This code fix handles all MDYNAME diagnostics (001-007) by extracting the required suffix
/// from the diagnostic properties and renaming the type accordingly.
///
/// Smart suffix handling:
/// - Detects partial suffix matches (e.g., "Config" as partial match for "Configuration")
/// - Replaces partial suffixes instead of appending (e.g., "UserConfig" → "UserConfiguration")
/// - Handles case variants (e.g., "DTO" → "Dto")
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddSuffixCodeFixProvider))]
[Shared]
public sealed class AddSuffixCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Maps partial suffixes to their full forms for smart replacement.
    /// Key: partial suffix (case-insensitive), Value: full suffix.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _partialSuffixMap =
        ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, new KeyValuePair<string, string>[]
        {
            // Configuration variants
            new("Config", "Configuration"),
            new("Cfg", "Configuration"),

            // Specification variants
            new("Spec", "Specification"),

            // Repository variants
            new("Repo", "Repository"),

            // Validator variants
            new("Valid", "Validator"),

            // Handler variants
            new("Handle", "Handler"),

            // Service variants (less common)
            new("Svc", "Service"),
            new("Srv", "Service"),

            // DTO variants (case normalization)
            new("DTO", "Dto"),
            new("Dtos", "Dto"),  // Plural to singular
        });

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [
        DiagnosticIds.MDYNAME001,
        DiagnosticIds.MDYNAME002,
        DiagnosticIds.MDYNAME003,
        DiagnosticIds.MDYNAME004,
        DiagnosticIds.MDYNAME005,
        DiagnosticIds.MDYNAME006,
        DiagnosticIds.MDYNAME007
    ];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics[0];
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration at the diagnostic location
        TypeDeclarationSyntax? typeDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        if (typeDeclaration is null)
        {
            return;
        }

        // Get the required suffix from diagnostic properties
        if (!diagnostic.Properties.TryGetValue(SuffixAnalyzerBase.RequiredSuffixProperty, out string? requiredSuffix) ||
            string.IsNullOrEmpty(requiredSuffix))
        {
            return;
        }

        string currentName = typeDeclaration.Identifier.Text;
        string newName = ComputeNewName(currentName, requiredSuffix!);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Rename to '{newName}'",
                createChangedSolution: ct => RenameTypeAsync(context.Document, typeDeclaration, newName, ct),
                equivalenceKey: $"AddSuffix_{requiredSuffix}"),
            diagnostic);
    }

    /// <summary>
    /// Computes the new name by intelligently handling partial suffix matches.
    /// </summary>
    /// <param name="currentName">The current type name.</param>
    /// <param name="requiredSuffix">The required suffix.</param>
    /// <returns>The new name with the correct suffix.</returns>
    private static string ComputeNewName(string currentName, string requiredSuffix)
    {
        // First, check if the name ends with a partial suffix that maps to the required suffix
        foreach (KeyValuePair<string, string> kvp in _partialSuffixMap)
        {
            string partialSuffix = kvp.Key;
            string fullSuffix = kvp.Value;

            // Only process if this partial maps to our required suffix
            if (!string.Equals(fullSuffix, requiredSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            // Check if current name ends with the partial suffix (case-insensitive)
            if (currentName.EndsWith(partialSuffix, StringComparison.OrdinalIgnoreCase))
            {
                // Replace the partial suffix with the full suffix
                string baseName = currentName.Substring(0, currentName.Length - partialSuffix.Length);
                return baseName + requiredSuffix;
            }
        }

        // No partial suffix match found, just append the required suffix
        return currentName + requiredSuffix;
    }

    private static async Task<Solution> RenameTypeAsync(
        Document document,
        TypeDeclarationSyntax typeDeclaration,
        string newName,
        CancellationToken cancellationToken)
    {
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document.Project.Solution;
        }

        ISymbol? symbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
        if (symbol is null)
        {
            return document.Project.Solution;
        }

        // RS0030: "Do not use banned APIs" - This warning targets source generators
        // that should use ForceRename, but for code fix providers, RenameSymbolAsync is
        // the documented and recommended API per Roslyn documentation:
        // https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix
        // RenameSymbolAsync handles all the complexities of cross-document and cross-project renaming.
#pragma warning disable RS0030
        Solution newSolution = await Renamer.RenameSymbolAsync(
            document.Project.Solution,
            symbol,
            new SymbolRenameOptions(),
            newName,
            cancellationToken).ConfigureAwait(false);
#pragma warning restore RS0030

        return newSolution;
    }
}
