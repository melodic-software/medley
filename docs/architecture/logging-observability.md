# Logging & Observability

Logging conventions, distributed tracing, and metrics patterns for the Medley modular monolith using .NET Aspire.

## Logging Conventions

### Source-Generated Logging (Preferred)

Use `[LoggerMessage]` attribute for high-performance logging:

```csharp
public static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} created")]
    public static partial void UserCreated(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rate limit exceeded for {Endpoint}")]
    public static partial void RateLimitExceeded(this ILogger logger, string endpoint);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process order {OrderId}")]
    public static partial void OrderProcessingFailed(this ILogger logger, Guid orderId, Exception ex);
}
```

**Why**: Avoids boxing, string allocations, and reflection at runtime. CA1848 analyzer enforces this.

### Log Levels

| Level | Use For | Example |
|-------|---------|---------|
| Trace | Detailed debugging (dev only) | Method entry/exit, variable values |
| Debug | Diagnostic information (dev only) | Cache hits, query timing |
| Information | Key business events | User login, order completed |
| Warning | Potential issues | Retry attempted, deprecated API |
| Error | Failures requiring attention | Unhandled exception, service unavailable |
| Critical | Application-wide failures | Database down, config missing |

### Structured Logging

1. **Use message templates** - Not string interpolation
   ```csharp
   // Good - structured
   logger.LogInformation("Order {OrderId} placed by {UserId}", orderId, userId);

   // Bad - loses structure
   logger.LogInformation($"Order {orderId} placed by {userId}");
   ```

2. **Include correlation IDs** for distributed tracing
   ```csharp
   using var scope = logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);
   ```

3. **Use semantic property names** in PascalCase
   ```csharp
   logger.LogInformation("Processing {ItemCount} items for {CustomerId}", count, customerId);
   ```

### Aspire Integration

With .NET Aspire, logging is automatically configured via ServiceDefaults:

```csharp
builder.AddServiceDefaults();  // Configures OpenTelemetry logging
```

OTLP exporter sends logs to Aspire dashboard and external observability platforms.

### Prohibited - Logging

- String interpolation in log messages (breaks structured logging)
- Logging sensitive data (PII, credentials, tokens)
- Logging inside hot paths without level checks
- Synchronous logging to slow sinks
- Using `Console.WriteLine` instead of ILogger

---

## Observability with .NET Aspire

Medley uses .NET Aspire for distributed application orchestration and observability.

### OpenTelemetry Integration

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

### Distributed Tracing

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

### Metrics

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

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<ExternalApiHealthCheck>("external-api");

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
```

### Aspire Dashboard

During development, Aspire dashboard provides:
- Real-time traces and spans
- Metrics visualization
- Structured logs
- Service dependency graph

Access via the URL displayed in console output when running `dotnet run --project src/Medley.AppHost`.
The port is dynamically assigned and varies between runs.

### Prohibited - Observability

- Logging secrets or PII in traces
- High-cardinality metric labels (user IDs, request IDs)
- Blocking on telemetry operations
- Disabling observability in production

## Related Documentation

- [Project Structure](project-structure.md) - Aspire host and service defaults projects
