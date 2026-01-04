# Feature Flags

## Overview

Feature flags (toggles) enable trunk-based development by allowing incomplete features to be merged and deployed without being visible to users.

## Library

[**Microsoft.FeatureManagement**](https://www.nuget.org/packages/Microsoft.FeatureManagement) - Official Microsoft library

> **Version Note**: As of January 2026, the latest stable version is 4.4.0.
> Always verify the current version on [NuGet](https://www.nuget.org/packages/Microsoft.FeatureManagement) before adding to your project.
>
> *Last verified: 2026-01-03*

## Installation

```bash
dotnet add package Microsoft.FeatureManagement
```

## Configuration

### appsettings.json

Use the `feature_management` schema (recommended for v4.0.0+):

```json
{
  "feature_management": {
    "feature_flags": [
      {
        "id": "NewDashboard",
        "enabled": false
      },
      {
        "id": "BetaFeatures",
        "enabled": true,
        "conditions": {
          "client_filters": [
            { "name": "Percentage", "parameters": { "Value": 10 } }
          ]
        }
      }
    ]
  }
}
```

> **Schema Note**: The `feature_management` schema (with `feature_flags` array) supports telemetry, variants, and conditions introduced in v4.0.0+. The legacy `FeatureManagement` schema still works but doesn't support these newer capabilities. See [Microsoft Learn](https://learn.microsoft.com/en-us/azure/azure-app-configuration/feature-management-dotnet-reference#feature-flags) for details.

### Program.cs

```csharp
builder.Services.AddFeatureManagement();
```

## Usage

### Basic Check

```csharp
if (await _featureManager.IsEnabledAsync("NewDashboard"))
{
    // New feature code
}
```

### Dependency Injection

```csharp
public class MyService(IFeatureManager featureManager)
{
    public async Task DoWorkAsync(CancellationToken ct = default)
    {
        if (await featureManager.IsEnabledAsync("NewDashboard", ct))
        {
            // New feature logic
        }
        else
        {
            // Existing logic
        }
    }
}
```

### Razor Components (Blazor)

```razor
@inject IFeatureManager FeatureManager
@implements IDisposable

@if (_showNewDashboard)
{
    <NewDashboard />
}
else
{
    <LegacyDashboard />
}

@code {
    private bool _showNewDashboard;
    private CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _showNewDashboard = await FeatureManager.IsEnabledAsync("NewDashboard", _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
```

## Trunk-Based Development Integration

1. **Merge incomplete features** behind flags
2. **Deploy to production** with flag disabled
3. **Enable progressively** (percentage rollout)
4. **Monitor and iterate** based on feedback
5. **Remove flag** once feature is stable

## Future Evolution

Can evolve to **LaunchDarkly** or similar for:
- Advanced targeting (user segments, geography)
- A/B testing
- Real-time flag updates without deployment
- Analytics and experimentation

---

*Last updated: 2026-01-03*
