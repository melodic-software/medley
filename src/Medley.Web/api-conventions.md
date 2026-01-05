<!-- Last reviewed: 2026-01-04 -->
<!-- ~1,200 tokens -->

# REST API Conventions

Medley uses Minimal APIs for HTTP endpoints with Duende.BFF for security.

## Endpoint Categories

| Category | URL Pattern | Auth | Purpose | Consumers |
|----------|-------------|------|---------|-----------|
| BFF Management | `/bff/*` | OIDC flows | Login, logout, session | Duende framework |
| Internal APIs | `/api/*` | Cookie + CSRF | Application business logic | Blazor (all modes), Hybrid |
| Outbound Proxies | via `MapRemoteBffApiEndpoint` | Cookie->Bearer | Call external services | BFF calling OUT |

**Note**: External APIs for mobile/M2M with JWT are NOT in Medley.Web scope.

### BFF Management Endpoints

Session management endpoints provided by Duende.BFF:

```csharp
app.MapBffManagementEndpoints();  // /bff/login, /bff/logout, /bff/user, /bff/silent-login
```

These endpoints handle OIDC flows and session state. **Never create custom endpoints under `/bff/`**.

### Internal APIs

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

### Outbound Proxies

For calling OUT to external services that require access tokens:

```csharp
// Outbound proxies - BFF proxies requests to external services with token injection
app.MapRemoteBffApiEndpoint("/api/external/payments", "https://payments.example.com")
    .RequireAccessToken(TokenType.User);  // Token attached from server session
```

BFF automatically translates the cookie session to a Bearer token for the outbound request.
Requires `builder.Services.AddBff().AddRemoteApis();` for YARP integration.

## API Versioning

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

## Endpoint Organization

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

## TypedResults

Use `TypedResults` for compile-time response type verification:

```csharp
async Task<Results<Ok<UserDto>, NotFound, BadRequest<ValidationProblemDetails>>> Handle(...)
```

## Response Codes

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

## Module Endpoint Registration

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

## Prohibited

- Returning `IActionResult` in Minimal APIs (use `Results<>`)
- Business logic in endpoint handlers (delegate to CQRS)
- Synchronous database calls
- Missing `CancellationToken` in async handlers
- Internal APIs without `.AsBffApiEndpoint()` (breaks CSRF protection)
- Custom endpoints under `/bff/*` (reserved for Duende.BFF)
- Version numbers in URL paths (use header-based versioning)
