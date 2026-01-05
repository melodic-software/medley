<!-- Last reviewed: 2026-01-04 -->
<!-- ~800 tokens -->

# Blazor Component Patterns

Medley uses Blazor Interactive Auto (Server + WebAssembly hybrid).

## Interactive Auto Behavior

**Important**: Auto picks Server or WASM at **initial render time**, not dynamically:
- First visit: Server-side (fast initial load, SignalR circuit)
- After WASM download: WebAssembly (runs in browser)
- WASM components **must** use HTTP APIs—no direct server access

| Render Mode | Communication | API Required? |
|-------------|---------------|---------------|
| Server | SignalR circuit | Could bypass, but use APIs for consistency |
| WebAssembly | HTTP only | **Yes** - no direct server access |
| Auto | Server initially, then WASM | **Yes** - WASM needs APIs after download |
| Hybrid/MAUI | HTTP only | **Yes** - mobile apps call APIs like WASM |

**Conclusion**: All `/api/*` endpoints are mandatory for WASM/Hybrid support.

## Component Structure

```
Components/
├── Pages/           # Routable pages (@page directive)
├── Layout/          # Layout components
├── Shared/          # Reusable components
└── Features/        # Feature-specific components
```

## Rules

1. **Use code-behind for complex logic**
   ```
   MyComponent.razor      # Markup only
   MyComponent.razor.cs   # Logic (partial class)
   ```

2. **Parameters must be public properties with [Parameter]**
   ```csharp
   [Parameter] public string Title { get; set; } = string.Empty;
   [Parameter] public EventCallback<string> OnChange { get; set; }
   ```

3. **Use CascadingParameter sparingly** - Prefer explicit parameters

4. **Dispose resources properly**
   ```csharp
   @implements IAsyncDisposable

   public async ValueTask DisposeAsync()
   {
       // Cleanup subscriptions, timers, etc.
   }
   ```

5. **Render mode selection**
   ```razor
   @* Server-side for initial load, then WebAssembly *@
   @rendermode InteractiveAuto

   @* Server-only for admin pages *@
   @rendermode InteractiveServer
   ```

## State Management

- Use cascading values for auth state
- Use scoped services for component-local state
- Use Fluxor or similar for complex global state

### State Persistence (.NET 10)

**Note**: `[PersistentState]` replaces the .NET 9 `SupplyParameterFromPersistentComponentState`
attribute and eliminates the need for manual `PersistAsJson`/`TryTakeFromJson` calls.

1. **`[PersistentState]` attribute** for automatic state persistence
   ```csharp
   // Default: state loads only on initial interactive render
   [PersistentState]
   public int? CurrentCount { get; set; }

   // For read-only data that should update during enhanced navigation:
   [PersistentState(AllowUpdates = true)]
   public WeatherForecast[]? Forecasts { get; set; }

   protected override async Task OnInitializedAsync()
   {
       // Use null-coalescing to avoid overwriting restored state
       Forecasts ??= await ForecastService.GetForecastAsync();
   }
   ```

2. **Stream rendering** for progressive content loading
   ```razor
   @attribute [StreamRendering]
   ```

## DbContext in Blazor

**Always use `IDbContextFactory<T>`** - never inject DbContext directly. See [src/CLAUDE.md](../CLAUDE.md#blazor-integration) for examples and rationale.

## Prohibited

- Direct DOM manipulation (use JS interop sparingly)
- Synchronous calls to async APIs
- Unbounded collections in component state
- Direct DbContext injection (use IDbContextFactory)
