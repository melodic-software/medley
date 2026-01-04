---
paths:
  - "**/Infrastructure/**/*.cs"
---

<!-- ~800 tokens -->

# EF Core Patterns

## Configuration

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

## Data Access Strategy (CQRS-Aligned)

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

## Rules

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

## Bulk Operations

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

## Modern EF Core Features

Features by version (use minimum version that includes needed feature):

### EF Core 7+ Features

1. **JSON columns** for complex property storage
   ```csharp
   builder.OwnsOne(x => x.Metadata, b => b.ToJson());
   ```

### EF Core 8+ Features

2. **Complex types** for inline value objects (no identity)
   ```csharp
   builder.ComplexProperty(x => x.Address);
   ```

### EF Core 10 Features

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

## Blazor Integration

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

## Prohibited

- `Include()` chains longer than 2 levels
- Lazy loading (disabled by default)
- Raw SQL without parameterization
- Exposing `IQueryable<T>` from repositories
- Generic CRUD repositories that duplicate DbSet APIs
- Direct DbContext injection in Blazor components (use IDbContextFactory)
