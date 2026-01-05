<!-- Last reviewed: 2026-01-04 -->
<!-- ~400 tokens -->
<!-- Lazy-loaded: Only included when working in src/Medley.Web/ directory -->

# Medley.Web - Blazor UI + BFF Host

Medley.Web is the single-host application serving both the Blazor UI and BFF (Backend for
Frontend) layer. It sits behind the **App Gateway** (`apps.melodicsoftware.com`) and handles
cookie-based authentication for browser clients.

**Scope**: This project does NOT host external JWT-authenticated APIs. Those route through the
separate API Gateway (`api.melodicsoftware.com`). See [BACKLOG.md](../../docs/BACKLOG.md#gateway-architecture-yarp-via-aspire) for gateway architecture.

---

## Detailed Documentation

| Topic | File | Content |
|-------|------|---------|
| Blazor Components | [blazor-patterns.md](blazor-patterns.md) | Interactive Auto, state management, component structure |
| REST APIs | [api-conventions.md](api-conventions.md) | Minimal APIs, endpoint patterns, TypedResults |
| Authentication | [bff-authentication.md](bff-authentication.md) | Duende BFF, CSRF, token handling |

---

## Gateway Architecture

```
                     EXTERNAL CLIENTS
                           |
             +-------------+-------------+
             |                           |
   apps.melodicsoftware.com    api.melodicsoftware.com
       (App Gateway:5000)        (API Gateway:5001)
             |                           |
     +-------+-------+           +-------+-------+
     |       |       |           |       |       |
  Blazor   BFF    Identity    Module   Module   External
    UI   Endpoints  Server    APIs     APIs     APIs
     +-------+-------+           +-------+-------+
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

## Quick Reference

### Why Internal APIs Exist

Blazor Interactive Auto supports multiple render modes. **WebAssembly and Hybrid/MAUI apps
cannot call server services directly**—they must use HTTP APIs.

**Conclusion**: All `/api/*` endpoints are mandatory for WASM/Hybrid support. Use consistent
API patterns across all render modes.

### Endpoint Categories

| Category | URL Pattern | Auth | Purpose |
|----------|-------------|------|---------|
| BFF Management | `/bff/*` | OIDC flows | Login, logout, session |
| Internal APIs | `/api/*` | Cookie + CSRF | Application business logic |
| Outbound Proxies | `MapRemoteBffApiEndpoint` | Cookie->Bearer | Call external services |

### Cross-Cutting Concerns

| Concern | Gateway (YARP) | Medley.Web (BFF) |
|---------|---------------|------------------|
| TLS Termination | Primary | Internal HTTP |
| CORS | Primary | Trusts gateway |
| Rate Limiting | Primary | Business throttling |
| CSRF | - | `.AsBffApiEndpoint()` |
| Authorization | Coarse | Fine-grained |

**Key insight**: Gateway blocks obvious bad actors; Medley.Web enforces business rules.

---

## Prohibited

See detailed documentation files for topic-specific prohibited patterns:
- [Blazor Prohibited](blazor-patterns.md#prohibited)
- [API Prohibited](api-conventions.md#prohibited)
- [Auth Prohibited](bff-authentication.md#prohibited)
