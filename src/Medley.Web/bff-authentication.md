<!-- Last reviewed: 2026-01-04 -->
<!-- ~600 tokens -->

# BFF Authentication Pattern

Medley uses Duende IdentityServer 7.4 with the BFF (Backend for Frontend) pattern.

## Architecture

```
Browser (Cookie)              BFF Host (Session + Token)         External API
      |                              |                                |
      | 1. Request with Cookie       |                                |
      |------------------------------->                                |
      |                              | 2. Resolve Access Token        |
      |                              |    from Server Session         |
      |                              |                                |
      |                              | 3. Proxy with Bearer Token     |
      |                              |------------------------------------>
      |                              |                                |
      | 4. Response                  | <--------------------------------- |
      | <----------------------------  |                                |
      |                              |                                |

         IdentityServer: Issues tokens, BFF stores in encrypted server-side session
```

## Rules

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

## Duende.BFF Configuration

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

## Passkey/WebAuthn (.NET 10)

.NET 10 includes built-in passkey support via ASP.NET Core Identity:

```csharp
builder.Services.Configure<IdentityPasskeyOptions>(options =>
{
    options.ServerDomain = "medley.app";
    options.AuthenticatorTimeout = TimeSpan.FromMinutes(3);
    options.UserVerificationRequirement = UserVerificationRequirement.Preferred;
});
```

## Cross-Cutting Concerns

Defense-in-depth: security is layered between gateway and service.

| Concern | Gateway (YARP) | Medley.Web (BFF) |
|---------|---------------|------------------|
| TLS Termination | Primary | Internal HTTP |
| CORS | Primary | Trusts gateway |
| Rate Limiting | Primary | Business throttling |
| CSRF | - | `.AsBffApiEndpoint()` |
| Authorization | Coarse | Fine-grained |
| Request Validation | Schema/size | Business rules |

**Key insight**: Gateway blocks obvious bad actors; Medley.Web enforces business rules.

## Prohibited

- Storing tokens in localStorage or sessionStorage
- Exposing access tokens to client-side JavaScript
- Disabling HTTPS in production
- Using symmetric keys for JWT signing in production
- Missing `X-CSRF` header on frontend API calls
