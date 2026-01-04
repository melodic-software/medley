<!-- Last reviewed: 2026-01-04 -->
<!-- ~3.0k tokens -->
<!-- Lazy-loaded: Only included when working in src/Medley.Web/ directory -->

# Medley.Web - Blazor UI + BFF Host

Medley.Web is the single-host application serving both the Blazor UI and BFF (Backend for
Frontend) layer. It sits behind the **App Gateway** (`apps.melodicsoftware.com`) and handles
cookie-based authentication for browser clients.

**Scope**: This project does NOT host external JWT-authenticated APIs. Those route through the
separate API Gateway (`api.melodicsoftware.com`). See [BACKLOG.md](../../docs/BACKLOG.md#gateway-architecture-yarp-via-aspire) for gateway architecture.

---

## Gateway Architecture

```
                     EXTERNAL CLIENTS
                           │
             ┌─────────────┴─────────────┐
             │                           │
   apps.melodicsoftware.com    api.melodicsoftware.com
       (App Gateway:5000)        (API Gateway:5001)
             │                           │
     ┌───────┼───────┐           ┌───────┼───────┐
     │       │       │           │       │       │
  Blazor   BFF    Identity    Module   Module   External
    UI   Endpoints  Server    APIs     APIs     APIs
     └───────┴───────┘           └───────┴───────┘
         Medley.Web                  (JWT Bearer)
       (Cookie + CSRF)
```

**Medley.Web responsibilities**: Blazor UI, BFF session management, internal APIs (cookie auth).

**Communication Channels**:

| Channel | Protocol | Routed Through |
|---------|----------|----------------|
| External HTTP | HTTPS | YARP Gateways |
| Internal Messaging | AMQP | RabbitMQ (not YARP) |
| Real-time (SignalR) | WebSocket | App Gateway (with affinity) |

---

## Why Internal APIs Exist

Blazor Interactive Auto supports multiple render modes. **WebAssembly and Hybrid/MAUI apps
cannot call server services directly**—they must use HTTP APIs:

| Render Mode | Communication | API Required? |
|-------------|---------------|---------------|
| Server | SignalR circuit | Could bypass, but use APIs for consistency |
| WebAssembly | HTTP only | **Yes** - no direct server access |
| Auto | Server initially, then WASM | **Yes** - WASM needs APIs after download |
| Hybrid/MAUI | HTTP only | **Yes** - mobile apps call APIs like WASM |

**Conclusion**: All `/api/*` endpoints are mandatory for WASM/Hybrid support. Use consistent
API patterns across all render modes.

---

## Blazor Component Patterns

Medley uses Blazor Interactive Auto (Server + WebAssembly hybrid).

### Interactive Auto Behavior

**Important**: Auto picks Server or WASM at **initial render time**, not dynamically:
- First visit: Server-side (fast initial load, SignalR circuit)
- After WASM download: WebAssembly (runs in browser)
- WASM components **must** use HTTP APIs—no direct server access

### Component Structure

```
Components/
├── Pages/           # Routable pages (@page directive)
├── Layout/          # Layout components
├── Shared/          # Reusable components
└── Features/        # Feature-specific components
```

### Rules

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

### State Management

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

### DbContext in Blazor

**Always use `IDbContextFactory<T>`** - never inject DbContext directly:

```csharp
@inject IDbContextFactory<AppDbContext> DbFactory

private async Task SaveChanges(CancellationToken ct = default)
{
    await using var context = await DbFactory.CreateDbContextAsync(ct);
    await context.SaveChangesAsync(ct);
}
```

**Why**: Blazor circuits outlive DbContext's intended lifespan.

---

## REST API Conventions

Medley uses Minimal APIs for HTTP endpoints with Duende.BFF for security.

### Endpoint Categories

| Category | URL Pattern | Auth | Purpose | Consumers |
|----------|-------------|------|---------|-----------|
| BFF Management | `/bff/*` | OIDC flows | Login, logout, session | Duende framework |
| Internal APIs | `/api/*` | Cookie + CSRF | Application business logic | Blazor (all modes), Hybrid |
| Outbound Proxies | via `MapRemoteBffApiEndpoint` | Cookie→Bearer | Call external services | BFF calling OUT |

**Note**: External APIs for mobile/M2M with JWT are NOT in Medley.Web scope—they route
through API Gateway.

#### BFF Management Endpoints

Session management endpoints provided by Duende.BFF:

```csharp
app.MapBffManagementEndpoints();  // /bff/login, /bff/logout, /bff/user, /bff/silent-login
```

These endpoints handle OIDC flows and session state. **Never create custom endpoints under `/bff/`**.

#### Internal APIs

Business logic endpoints that serve the Blazor frontend. Protected by BFF cookie
authentication—no tokens exposed to browser JavaScript:

```csharp
// Internal APIs - same-host endpoints protected by session cookie
var orders = app.MapGroup("/api/orders")
    .AsBffApiEndpoint()      // REQUIRED: BFF session + CSRF protection
    .RequireAuthorization()  // OPTIONAL: Policy-based authorization
    .WithTags("Orders");

orders.MapGet("/", GetOrders);
orders.MapPost("/", CreateOrder);
```

**Key rules for internal APIs:**
- Always use `.AsBffApiEndpoint()` for CSRF protection
- Handlers delegate to CQRS commands/queries (no business logic in endpoints)
- Return `Results<>` or `TypedResults` for OpenAPI generation
- Frontend must send `X-CSRF: 1` header with all requests

#### Outbound Proxies

For calling OUT to external services that require access tokens (e.g., third-party payment APIs):

```csharp
// Outbound proxies - BFF proxies requests to external services with token injection
app.MapRemoteBffApiEndpoint("/api/external/payments", "https://payments.example.com")
    .RequireAccessToken(TokenType.User);  // Token attached from server session
```

BFF automatically translates the cookie session to a Bearer token for the outbound request.
Requires `builder.Services.AddBff().AddRemoteApis();` for YARP integration.

**Note**: This is for calling OUT, not for exposing APIs to external consumers.

### API Versioning

Use header-based versioning (no version pollution in URLs):

```http
GET /api/orders
Api-Version: 2
```

Configure via `Asp.Versioning.Http`:

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("Api-Version");
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});
```

### Endpoint Organization

1. **Group endpoints** with `MapGroup()` and BFF protection
   ```csharp
   var users = app.MapGroup("/api/users")
       .AsBffApiEndpoint()  // Cookie-protected, requires X-CSRF header
       .WithTags("Users");

   users.MapGet("/", GetUsers);
   users.MapGet("/{id:guid}", GetUserById);
   users.MapPost("/", CreateUser);
   ```

2. **Static handler methods** in endpoint classes
   ```csharp
   public static class UserEndpoints
   {
       public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
       {
           var group = routes.MapGroup("/api/users")
               .AsBffApiEndpoint()
               .RequireAuthorization();

           group.MapGet("/{id:guid}", GetUserById);
       }

       private static async Task<Results<Ok<UserDto>, NotFound>> GetUserById(
           Guid id,
           IMediator mediator,
           CancellationToken ct)
       {
           var result = await mediator.Send(new GetUserByIdQuery(id), ct);
           return result.Match<Results<Ok<UserDto>, NotFound>>(
               user => TypedResults.Ok(user),
               _ => TypedResults.NotFound());
       }
   }
   ```

### TypedResults

Use `TypedResults` for compile-time response type verification:

```csharp
async Task<Results<Ok<UserDto>, NotFound, BadRequest<ValidationProblemDetails>>> Handle(...)
```

### Response Codes

| Code | Meaning | Use For |
|------|---------|---------|
| 200 OK | Success | GET requests returning data |
| 201 Created | Resource created | POST with Location header |
| 204 No Content | Success, no body | DELETE, PUT with no response |
| 400 Bad Request | Validation failure | Invalid input |
| 401 Unauthorized | Not authenticated | Missing/invalid session cookie |
| 403 Forbidden | Not authorized | Insufficient permissions |
| 404 Not Found | Resource missing | Non-existent resource |
| 409 Conflict | State conflict | Duplicate key, concurrent modification |
| 422 Unprocessable | Semantic error | Valid syntax, failed business logic |

### Module Endpoint Registration

Each module defines its own endpoint mappings. Register in `Program.cs` with proper ordering:

```csharp
// Program.cs - Endpoint registration order matters

// 1. BFF management endpoints (must be first)
app.MapBffManagementEndpoints();

// 2. Module API endpoints (internal APIs)
app.MapUserEndpoints();      // from Users.Contracts
app.MapOrderEndpoints();     // from Orders.Contracts
app.MapProductEndpoints();   // from Products.Contracts

// 3. Outbound proxies (for calling external services)
app.MapRemoteBffApiEndpoint("/api/external/payments", "https://payments.example.com")
    .RequireAccessToken(TokenType.User);
```

---

## Authentication (BFF Pattern)

Medley uses Duende IdentityServer 7.4 with the BFF pattern.

### Architecture

```
Browser (Cookie)              BFF Host (Session + Token)         External API
      │                              │                                │
      │ 1. Request with Cookie       │                                │
      │────────────────────────────→ │                                │
      │                              │ 2. Resolve Access Token        │
      │                              │    from Server Session         │
      │                              │                                │
      │                              │ 3. Proxy with Bearer Token     │
      │                              │────────────────────────────────→
      │                              │                                │
      │ 4. Response                  │ ←────────────────────────────── │
      │ ←──────────────────────────  │                                │
      │                              │                                │

         IdentityServer: Issues tokens, BFF stores in encrypted server-side session
```

### Rules

1. **BFF pattern for browser clients** - Never expose tokens to browser JavaScript
   ```csharp
   services.AddBff();
   app.UseBff();
   ```

2. **Secure token storage** - Server-side session, not localStorage

3. **Use policy-based authorization**
   ```csharp
   [Authorize(Policy = "RequireAdminRole")]
   public class AdminController : ControllerBase { }
   ```

### Duende.BFF Configuration

1. **Service registration** (in `Program.cs`)
   ```csharp
   builder.Services.AddBff()
       .AddRemoteApis();  // Required for MapRemoteBffApiEndpoint
   ```

2. **Middleware order** (critical)
   ```csharp
   app.UseAuthentication();
   app.UseBff();  // Must come after UseAuthentication
   app.UseAuthorization();
   ```

3. **CSRF protection** - Required header for all internal API calls
   ```csharp
   // Blazor HttpClient setup
   httpClient.DefaultRequestHeaders.Add("X-CSRF", "1");
   ```

   ```javascript
   // JavaScript fetch
   fetch('/api/data', { headers: { 'X-CSRF': '1' } });
   ```

### Passkey/WebAuthn (.NET 10)

.NET 10 includes built-in passkey support via ASP.NET Core Identity:

```csharp
builder.Services.Configure<IdentityPasskeyOptions>(options =>
{
    options.ServerDomain = "medley.app";
    options.AuthenticatorTimeout = TimeSpan.FromMinutes(3);
    options.UserVerificationRequirement = UserVerificationRequirement.Preferred;
});
```

---

## Cross-Cutting Concerns

Defense-in-depth: security is layered between gateway and service.

| Concern | Gateway (YARP) | Medley.Web (BFF) |
|---------|---------------|------------------|
| TLS Termination | ✅ Primary | ❌ Internal HTTP |
| CORS | ✅ Primary | ❌ Trusts gateway |
| Rate Limiting | ✅ Primary | ⚠️ Business throttling |
| CSRF | ❌ | ✅ `.AsBffApiEndpoint()` |
| Authorization | ✅ Coarse | ✅ Fine-grained |
| Request Validation | ✅ Schema/size | ✅ Business rules |

**Key insight**: Gateway blocks obvious bad actors; Medley.Web enforces business rules.

---

## Prohibited

### Blazor
- Direct DOM manipulation (use JS interop sparingly)
- Synchronous calls to async APIs
- Unbounded collections in component state
- Direct DbContext injection (use IDbContextFactory)

### API
- Returning `IActionResult` in Minimal APIs (use `Results<>`)
- Business logic in endpoint handlers (delegate to CQRS)
- Synchronous database calls
- Missing `CancellationToken` in async handlers
- Internal APIs without `.AsBffApiEndpoint()` (breaks CSRF protection)
- Custom endpoints under `/bff/*` (reserved for Duende.BFF)
- Version numbers in URL paths (use header-based versioning)

### Authentication
- Storing tokens in localStorage or sessionStorage
- Exposing access tokens to client-side JavaScript
- Disabling HTTPS in production
- Using symmetric keys for JWT signing in production
- Missing `X-CSRF` header on frontend API calls
