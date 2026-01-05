namespace SharedKernel.Analyzers.Attributes;

/// <summary>
/// Declares the architectural style used by a module assembly.
/// </summary>
/// <remarks>
/// Apply this attribute at the assembly level to configure how analyzers
/// enforce architectural rules for the module.
///
/// <example>
/// <code>
/// // In AssemblyInfo.cs or any file in the module
/// [assembly: ModuleArchitecture(ModuleStyle.VerticalSlicesOnly)]
/// </code>
/// </example>
///
/// When <see cref="ModuleStyle.VerticalSlicesOnly"/> or <see cref="ModuleStyle.Custom"/>
/// is specified, layer dependency rules (MDYARCH003-005) are automatically disabled
/// for the assembly.
///
/// Alternative: Use .editorconfig for project-level configuration:
/// <code>
/// [src/Modules/SimpleModule/**/*.cs]
/// dotnet_diagnostic.MDYARCH003.severity = none
/// dotnet_diagnostic.MDYARCH004.severity = none
/// dotnet_diagnostic.MDYARCH005.severity = none
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class ModuleArchitectureAttribute : Attribute
{
    /// <summary>
    /// Gets the architectural style declared for this module.
    /// </summary>
    public ModuleStyle Style { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleArchitectureAttribute"/> class.
    /// </summary>
    /// <param name="style">The architectural style used by this module.</param>
    public ModuleArchitectureAttribute(ModuleStyle style)
    {
        Style = style;
    }
}
