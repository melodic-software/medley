---
paths:
  - "**/*.cs"
---

<!-- ~400 tokens -->

# Observability with .NET Aspire

Medley uses .NET Aspire for distributed application orchestration and observability.

## OpenTelemetry Integration

Aspire ServiceDefaults configures OpenTelemetry automatically:

```csharp
// In each service
builder.AddServiceDefaults();

// ServiceDefaults project configures:
// - Logging (OTLP exporter)
// - Metrics (OTLP exporter)
// - Tracing (OTLP exporter)
// - Health checks
```

## Distributed Tracing

1. **Automatic instrumentation** via Aspire
   - HTTP requests (incoming/outgoing)
   - Database queries (EF Core)
   - Message bus operations

2. **Custom spans** using Activity API
   ```csharp
   using var activity = ActivitySource.StartActivity("ProcessOrder");
   activity?.SetTag("order.id", orderId);
   activity?.SetTag("order.total", orderTotal);

   // Business logic here

   activity?.SetStatus(ActivityStatusCode.Ok);
   ```

3. **Correlation propagation** - Automatic via W3C trace context

## Metrics

1. **Use IMeterFactory** for custom metrics
   ```csharp
   public class OrderMetrics(IMeterFactory meterFactory)
   {
       private readonly Counter<long> _ordersPlaced = meterFactory
           .Create("Medley.Orders")
           .CreateCounter<long>("orders_placed_total");

       public void RecordOrderPlaced() => _ordersPlaced.Add(1);
   }
   ```

2. **Naming conventions** - Use snake_case with units
   - `http_requests_total`
   - `order_processing_duration_seconds`
   - `cache_hits_total`

3. **Standard dimensions** - Include service, endpoint, status
   ```csharp
   _requestDuration.Record(elapsed,
       new("service", "orders"),
       new("endpoint", "/api/orders"),
       new("status", "success"));
   ```

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<ExternalApiHealthCheck>("external-api");

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
```

## Aspire Dashboard

During development, Aspire dashboard provides:
- Real-time traces and spans
- Metrics visualization
- Structured logs
- Service dependency graph

Access via the URL displayed in console output when running `dotnet run --project src/Medley.AppHost`.
The port is dynamically assigned and varies between runs.

## Prohibited

- Logging secrets or PII in traces
- High-cardinality metric labels (user IDs, request IDs)
- Blocking on telemetry operations
- Disabling observability in production
