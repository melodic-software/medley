# MDYASYNC003: Async void method

| Property | Value |
|----------|-------|
| **Rule ID** | MDYASYNC003 |
| **Category** | Medley.Async |
| **Severity** | Error |
| **Enabled** | Yes |
| **Code Fix** | No |

## Cause

A method is declared as `async void` instead of `async Task`.

## Rule description

Async void methods cannot be awaited and exceptions thrown within them are unobservable (they crash the process on .NET).

The only valid use of `async void` is for event handlers.

## How to fix violations

Change `async void` to `async Task`:

```csharp
// Bad - async void
public async void ProcessOrderAsync(OrderId id)
{
    var order = await _repo.GetByIdAsync(id);
    // If this throws, the exception is lost!
}

// Good - async Task
public async Task ProcessOrderAsync(OrderId id)
{
    var order = await _repo.GetByIdAsync(id);
    // Exceptions can be caught by awaiting caller
}

// Acceptable - event handler
private async void OnButtonClick(object sender, EventArgs e)
{
    try
    {
        await ProcessAsync();
    }
    catch (Exception ex)
    {
        // Handle exception - important in event handlers!
        _logger.LogError(ex, "Processing failed");
    }
}
```

## When to suppress

Suppress only for true event handlers with proper try/catch:

```csharp
#pragma warning disable MDYASYNC003
private async void OnStartup(object sender, StartupEventArgs e)
{
    try
    {
        await InitializeAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Startup failed");
        Application.Current.Shutdown(1);
    }
}
#pragma warning restore MDYASYNC003
```

## Related rules

- [MDYASYNC001](MDYASYNC001.md) - Missing CancellationToken
- [MDYRES003](MDYRES003.md) - Result not awaited
