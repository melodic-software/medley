---
paths:
  - "**/*.cs"
---

<!-- ~300 tokens -->

# Logging Conventions

## Source-Generated Logging (Preferred)

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

## Log Levels

| Level | Use For | Example |
|-------|---------|---------|
| Trace | Detailed debugging (dev only) | Method entry/exit, variable values |
| Debug | Diagnostic information (dev only) | Cache hits, query timing |
| Information | Key business events | User login, order completed |
| Warning | Potential issues | Retry attempted, deprecated API |
| Error | Failures requiring attention | Unhandled exception, service unavailable |
| Critical | Application-wide failures | Database down, config missing |

## Structured Logging

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

## Aspire Integration

With .NET Aspire, logging is automatically configured via ServiceDefaults:

```csharp
builder.AddServiceDefaults();  // Configures OpenTelemetry logging
```

OTLP exporter sends logs to Aspire dashboard and external observability platforms.

## Prohibited

- String interpolation in log messages (breaks structured logging)
- Logging sensitive data (PII, credentials, tokens)
- Logging inside hot paths without level checks
- Synchronous logging to slow sinks
- Using `Console.WriteLine` instead of ILogger
