namespace SharedKernel.Analyzers.Attributes;

/// <summary>
/// Defines the architectural style used by a module.
/// </summary>
/// <remarks>
/// Different modules may use different architectural styles based on their complexity.
/// This enum allows modules to declare their style so analyzers can apply appropriate rules.
/// </remarks>
public enum ModuleStyle
{
    /// <summary>
    /// Full Clean Architecture with Domain, Application, Infrastructure layers.
    /// All layer dependency rules apply.
    /// </summary>
    CleanArchitecture = 0,

    /// <summary>
    /// Clean Architecture layers with vertical slice organization within Application layer.
    /// All layer dependency rules apply.
    /// </summary>
    CleanArchitectureWithVerticalSlices = 1,

    /// <summary>
    /// Vertical slices only - single project with feature-based organization.
    /// Layer dependency rules are disabled.
    /// </summary>
    VerticalSlicesOnly = 2,

    /// <summary>
    /// Custom architecture - all layer rules disabled, relies on manual review.
    /// Use sparingly for special cases.
    /// </summary>
    Custom = 3
}
