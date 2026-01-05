<!-- Last reviewed: 2026-01-04 -->
<!-- ~2,200 tokens -->
<!-- Lazy-loaded: Only included when working in src/ Infrastructure projects -->

# Infrastructure Patterns

Context-specific guidance for Infrastructure layer development: logging, observability, and database access patterns.

---

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

---

## EF Core Patterns

### Configuration

1. **Use Fluent API over attributes**
   ```csharp
   public class UserConfiguration : IEntityTypeConfiguration<User>
   {
       public void Configure(EntityTypeBuilder<User> builder)
       {
           builder.ToTable("Users", "Users");  // Schema per module
           builder.HasKey(x => x.Id);
           builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
       }
   }
   ```

2. **Apply configurations from assembly**
   ```csharp
   modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
   ```

### Data Access Strategy (CQRS-Aligned)

**Reads (Queries)**: Direct DbContext is preferred. Queries are simple, read-only, and
don't need the abstraction overhead. Use `AsNoTracking()` for performance.

```csharp
// Query handlers: DbContext directly, pragmatic and fast
public class GetUserByIdHandler(AppDbContext context)
{
    public async Task<User?> Handle(GetUserByIdQuery query, CancellationToken ct)
        => await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.Id, ct);
}
```

**Writes (Commands)**: Use repository abstractions, especially in DDD contexts.
This decouples domain logic from EF Core and enables future ORM swaps.

```csharp
// Command handlers: Repository for writes, DDD aggregate boundaries
public class CreateUserHandler(IUserRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<Result<UserId>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var user = User.Create(command.Email, command.Name);
        await repository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return user.Id;
    }
}

// Repository interface in Domain layer
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    void Remove(User user);
}
```

### Rules

1. **Queries use DbContext directly** - Fast, pragmatic, read-only
2. **Commands use repositories** - Decoupled, testable, DDD-friendly

3. **Use strongly-typed IDs** (readonly record struct recommended)
   ```csharp
   public readonly record struct UserId(Guid Value);

   // EF Core value conversion
   builder.Property(x => x.Id)
       .HasConversion(id => id.Value, value => new UserId(value));
   ```
   Consider: `StronglyTypedId` NuGet package for auto-generation.

4. **Configure value objects as owned types**
   ```csharp
   builder.OwnsOne(x => x.Address);
   ```

5. **Use query filters for soft delete**
   ```csharp
   builder.HasQueryFilter(x => !x.IsDeleted);
   ```

6. **Migrations per module**
   ```bash
   dotnet ef migrations add InitialCreate --context UsersDbContext --output-dir Migrations/Users
   ```

### Bulk Operations

Use EF Core 7+ bulk methods for performance:

```csharp
// Bulk update without loading entities
await context.Users
    .Where(u => u.LastLogin < cutoff)
    .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, false));

// Bulk delete
await context.AuditLogs
    .Where(l => l.CreatedAt < archiveDate)
    .ExecuteDeleteAsync();
```

### Modern EF Core Features

Features by version (use minimum version that includes needed feature):

#### EF Core 7+ Features

1. **JSON columns** for complex property storage
   ```csharp
   builder.OwnsOne(x => x.Metadata, b => b.ToJson());
   ```

#### EF Core 8+ Features

2. **Complex types** for inline value objects (no identity)
   ```csharp
   builder.ComplexProperty(x => x.Address);
   ```

#### EF Core 10 Features

3. **Named query filters** for multi-tenancy and soft-delete
   ```csharp
   // Define named filters (use constants to avoid typos)
   public static class Filters
   {
       public const string SoftDelete = "SoftDelete";
       public const string Tenant = "Tenant";
   }

   builder.HasQueryFilter(Filters.SoftDelete, x => !x.IsDeleted);
   builder.HasQueryFilter(Filters.Tenant, x => x.TenantId == _tenantId);

   // Selectively disable in queries (array syntax)
   context.Users.IgnoreQueryFilters([Filters.SoftDelete]).ToListAsync();

   // Disable all filters (parameterless overload)
   context.Users.IgnoreQueryFilters().ToListAsync();
   ```

4. **LeftJoin/RightJoin operators** for explicit outer joins
   ```csharp
   var query = context.Orders
       .LeftJoin(context.Customers,
           o => o.CustomerId,
           c => c.Id,
           (order, customer) => new { order, customer });
   ```

5. **Vector search** for AI/ML similarity queries
   ```csharp
   // Property configuration
   builder.Property(x => x.Embedding).HasColumnType("vector(1536)");

   // Query with vector distance
   var similar = await context.Products
       .OrderBy(p => EF.Functions.VectorDistance(p.Embedding, searchVector))
       .Take(10)
       .ToListAsync();
   ```

6. **ExecuteUpdate for JSON** properties
   ```csharp
   await context.Users
       .Where(u => u.Id == id)
       .ExecuteUpdateAsync(s => s
           .SetProperty(u => u.Preferences.Theme, "dark")
           .SetProperty(u => u.Preferences.Locale, "en-US"));
   ```

7. **SQL injection analyzer** (compile-time warnings for unsafe SQL)
   - Enabled by default in EF Core 10
   - Warns when raw SQL uses string interpolation unsafely

### Blazor Integration

Use `IDbContextFactory<T>` for Blazor components (not direct DbContext injection):

```csharp
// Registration
services.AddDbContextFactory<AppDbContext>(options => ...);

// Component usage
@inject IDbContextFactory<AppDbContext> DbFactory

@code {
    private async Task LoadData()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        users = await context.Users.ToListAsync();
    }
}
```

**Why**: Blazor Server circuits outlive DbContext's intended lifespan. Factory creates short-lived contexts per operation.

### Prohibited - EF Core

- `Include()` chains longer than 2 levels
- Lazy loading (disabled by default)
- Raw SQL without parameterization
- Exposing `IQueryable<T>` from repositories
- Generic CRUD repositories that duplicate DbSet APIs
- Direct DbContext injection in Blazor components (use IDbContextFactory)
