using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharedKernel.Analyzers.Analyzers.Architecture;

/// <summary>
/// Detects direct cross-module references that bypass the Contracts layer.
/// Modules should only reference other modules via their .Contracts projects.
/// </summary>
/// <remarks>
/// Rule: MDYARCH001
/// Severity: Error
///
/// Modules in a modular monolith must maintain isolation. Direct references
/// between module internals (Domain, Application, Infrastructure) violate
/// this boundary. Only Contracts projects may be referenced across modules.
///
/// Allowed patterns:
/// - Same module references (e.g., Orders.Application → Orders.Domain)
/// - Contracts references (e.g., Orders.Application → Users.Contracts)
/// - SharedKernel references (any module → SharedKernel.*)
///
/// Forbidden patterns:
/// - Cross-module internal references (e.g., Orders.Application → Users.Domain)
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CrossModuleReferenceAnalyzer : DiagnosticAnalyzer
{

    /// <summary>
    /// Pattern to extract module name and layer from namespace.
    /// Matches: ModuleName.Layer or Medley.Modules.ModuleName.Layer
    /// Groups: 1=ModuleName, 2=Layer
    /// </summary>
    private static readonly Regex _moduleNamespacePattern = new(
        @"^(?:.*\.)?Modules\.(\w+)\.(\w+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.MDYARCH001_CrossModuleDirectReference];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for compilation start to cache module information
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // Determine the current module from the assembly name
        string? assemblyName = context.Compilation.AssemblyName;
        if (string.IsNullOrEmpty(assemblyName))
        {
            return;
        }

        // Extract module name from assembly (e.g., "Orders.Application" → "Orders")
        ModuleInfo? currentModule = ParseModuleFromAssembly(assemblyName!);
        if (currentModule is null)
        {
            // Not a module assembly, skip analysis
            return;
        }

        // Register symbol action to analyze type references
        context.RegisterSymbolAction(
            ctx => AnalyzeNamedType(ctx, currentModule),
            SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, ModuleInfo currentModule)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Check base types
        INamedTypeSymbol? baseType = namedType.BaseType;
        while (baseType is not null && baseType.SpecialType == SpecialType.None)
        {
            CheckTypeReference(context, currentModule, baseType, namedType);
            baseType = baseType.BaseType;
        }

        // Check implemented interfaces
        foreach (INamedTypeSymbol iface in namedType.AllInterfaces)
        {
            CheckTypeReference(context, currentModule, iface, namedType);
        }

        // Check member types (fields, properties, method parameters/returns)
        foreach (ISymbol member in namedType.GetMembers())
        {
            ITypeSymbol? memberType = member switch
            {
                IFieldSymbol field => field.Type,
                IPropertySymbol prop => prop.Type,
                IMethodSymbol method => method.ReturnType,
                _ => null
            };

            if (memberType is INamedTypeSymbol namedMemberType)
            {
                CheckTypeReference(context, currentModule, namedMemberType, namedType);

                // Check generic type arguments
                foreach (ITypeSymbol typeArg in namedMemberType.TypeArguments)
                {
                    if (typeArg is INamedTypeSymbol namedTypeArg)
                    {
                        CheckTypeReference(context, currentModule, namedTypeArg, namedType);
                    }
                }
            }

            // Check method parameters
            if (member is IMethodSymbol methodSymbol)
            {
                foreach (IParameterSymbol param in methodSymbol.Parameters)
                {
                    if (param.Type is INamedTypeSymbol paramType)
                    {
                        CheckTypeReference(context, currentModule, paramType, namedType);
                    }
                }
            }
        }
    }

    private static void CheckTypeReference(
        SymbolAnalysisContext context,
        ModuleInfo currentModule,
        INamedTypeSymbol referencedType,
        INamedTypeSymbol containingType)
    {
        // Skip system/framework types
        string? referencedNamespace = referencedType.ContainingNamespace?.ToDisplayString();
        if (string.IsNullOrEmpty(referencedNamespace))
        {
            return;
        }

        if (referencedNamespace!.StartsWith("System", StringComparison.Ordinal) ||
            referencedNamespace.StartsWith("Microsoft", StringComparison.Ordinal))
        {
            return;
        }

        // Skip SharedKernel references (always allowed)
        if (referencedNamespace.Contains("SharedKernel", StringComparison.Ordinal))
        {
            return;
        }

        // Parse the referenced type's module info
        ModuleInfo? referencedModule = ParseModuleFromNamespace(referencedNamespace);
        if (referencedModule is null)
        {
            // Referenced type is not in a module namespace
            return;
        }

        // Same module references are always allowed
        if (string.Equals(currentModule.ModuleName, referencedModule.ModuleName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Cross-module reference detected - check if it's through Contracts
        if (string.Equals(referencedModule.LayerName, LayerNames.Contracts, StringComparison.OrdinalIgnoreCase))
        {
            // Contracts references are allowed
            return;
        }

        // Violation: direct cross-module reference to internal layer
        if (LayerNames.InternalLayers.Contains(referencedModule.LayerName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MDYARCH001_CrossModuleDirectReference,
                containingType.Locations[0],
                currentModule.ModuleName,
                $"{referencedModule.ModuleName}.{referencedModule.LayerName}"));
        }
    }

    private static ModuleInfo? ParseModuleFromAssembly(string assemblyName)
    {
        // Expected formats:
        // - "ModuleName.Layer" (e.g., "Orders.Application")
        // - "Medley.Modules.ModuleName.Layer"
        string[] parts = assemblyName.Split('.');

        // Find "Modules" segment or assume first.second pattern
        int modulesIndex = Array.IndexOf(parts, "Modules");
        if (modulesIndex >= 0 && modulesIndex + 2 < parts.Length)
        {
            return new ModuleInfo(parts[modulesIndex + 1], parts[modulesIndex + 2]);
        }

        // Fallback: assume ModuleName.Layer format
        if (parts.Length >= 2)
        {
            string potentialLayer = parts[parts.Length - 1];
            if (LayerNames.InternalLayers.Contains(potentialLayer) || potentialLayer == LayerNames.Contracts)
            {
                return new ModuleInfo(parts[parts.Length - 2], potentialLayer);
            }
        }

        return null;
    }

    private static ModuleInfo? ParseModuleFromNamespace(string @namespace)
    {
        Match match = _moduleNamespacePattern.Match(@namespace);
        if (match.Success)
        {
            return new ModuleInfo(match.Groups[1].Value, match.Groups[2].Value);
        }

        // Fallback: check for simple ModuleName.Layer pattern
        string[] parts = @namespace.Split('.');
        for (int i = 0; i < parts.Length - 1; i++)
        {
            string potentialLayer = parts[i + 1];
            if (LayerNames.InternalLayers.Contains(potentialLayer) || potentialLayer == LayerNames.Contracts)
            {
                return new ModuleInfo(parts[i], potentialLayer);
            }
        }

        return null;
    }

    /// <summary>
    /// Represents module identification information.
    /// </summary>
    private sealed record ModuleInfo(string ModuleName, string LayerName);
}
