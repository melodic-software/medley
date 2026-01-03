# Feature Flags

## Overview

Feature flags (toggles) enable trunk-based development by allowing incomplete features to be merged and deployed without being visible to users.

## Library

**Microsoft.FeatureManagement** - Official Microsoft library (v4.4.0+)

## Installation

```bash
dotnet add package Microsoft.FeatureManagement
```

## Configuration

### appsettings.json

```json
{
  "FeatureManagement": {
    "NewDashboard": false,
    "BetaFeatures": {
      "EnabledFor": [
        { "Name": "Percentage", "Parameters": { "Value": 10 } }
      ]
    }
  }
}
```

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
public class MyService
{
    private readonly IFeatureManager _featureManager;

    public MyService(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task DoWork()
    {
        if (await _featureManager.IsEnabledAsync("NewDashboard"))
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

@if (await FeatureManager.IsEnabledAsync("NewDashboard"))
{
    <NewDashboard />
}
else
{
    <LegacyDashboard />
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
