# Module Patterns

Guidelines for developing feature modules in the Medley modular monolith.

## Module Structure

Each module follows **vertical slice architecture** within **clean architecture** layers:

```
Modules/{ModuleName}/
├── {ModuleName}.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Repositories/           # Interfaces only
│   └── Specifications/
│
├── {ModuleName}.Application/
│   ├── Features/               # Vertical slices
│   │   └── {FeatureName}/
│   │       ├── {Feature}Command.cs
│   │       ├── {Feature}Handler.cs
│   │       ├── {Feature}Validator.cs  # FluentValidation inline
│   │       └── {Feature}Response.cs
│   ├── Mappings/
│   └── DependencyInjection.cs
│
├── {ModuleName}.Infrastructure/
│   ├── Persistence/
│   │   ├── {ModuleName}DbContext.cs
│   │   ├── Repositories/
│   │   └── Configurations/
│   ├── Services/               # External integrations
│   └── DependencyInjection.cs
│
└── {ModuleName}.Contracts/     # PUBLIC API for other modules
    ├── IntegrationEvents/
    └── Dtos/
```

## Vertical Slices

Features are organized by use case, not by type:

```
Features/
├── CreateUser/
│   ├── CreateUserCommand.cs
│   ├── CreateUserHandler.cs
│   ├── CreateUserValidator.cs
│   └── CreateUserResponse.cs
├── GetUser/
│   ├── GetUserQuery.cs
│   ├── GetUserHandler.cs
│   └── UserDto.cs
└── UpdateEmail/
    ├── UpdateEmailCommand.cs
    ├── UpdateEmailHandler.cs
    └── UpdateEmailValidator.cs
```

**Benefits**:
- Related code stays together
- Easy to understand feature scope
- Simple to add/remove features

## Module Isolation Rules

1. **Direct references prohibited** between modules
2. **Contracts only** for cross-module DTOs/events
3. **Integration events** for async communication
4. **No shared database tables** - Each module owns its schema

### Valid Dependencies

```
Users.Domain         → SharedKernel
Users.Application    → Users.Domain, SharedKernel.Application
Users.Infrastructure → Users.Application, SharedKernel.Infrastructure
Users.Contracts      → SharedKernel.Contracts (integration events only)
```

### Invalid Dependencies

```
Users.Application    → Orders.Domain (PROHIBITED)
Users.Infrastructure → Orders.Infrastructure (PROHIBITED)
Users.Domain         → Any other module (PROHIBITED)
```

## Cross-Module Communication

### Via Integration Events

```csharp
// Users.Contracts/IntegrationEvents/
public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    DateTime CreatedAt) : IntegrationEvent;

// Orders.Application/Features/CreateOrder/
public class UserCreatedEventHandler : INotificationHandler<UserCreatedIntegrationEvent>
{
    public async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken ct)
    {
        // React to user creation in Orders module
    }
}
```

### Via Contracts DTOs

```csharp
// Users.Contracts/Dtos/
public record UserSummaryDto(Guid Id, string Email, string Name);

// Other modules can use this DTO without referencing Users.Domain
```

## Database Schema Pattern

Each module owns its database schema:

```sql
-- Users module
CREATE SCHEMA [Users];
CREATE TABLE [Users].[User] (...);
CREATE TABLE [Users].[UserRole] (...);

-- Orders module
CREATE SCHEMA [Orders];
CREATE TABLE [Orders].[Order] (...);
CREATE TABLE [Orders].[OrderItem] (...);
```

EF Core migrations per module:

```bash
dotnet ef migrations add MigrationName \
    --project src/Modules/Users/Users.Infrastructure \
    --startup-project src/Medley.Web
```

## Module Architectural Flexibility

Modules can use different architectural styles based on complexity. The Roslyn analyzers support these styles with configurable rule enforcement.

### Supported Styles

| Style | Description | Layer Rule Enforcement |
|-------|-------------|------------------------|
| **Full Clean Architecture** | Domain/Application/Infrastructure/Contracts | All layer rules apply |
| **Clean + Vertical Slices** | Layers with Features/ organization | All layer rules apply |
| **Vertical Slices Only** | Single project, feature-based | Layer rules disabled |
| **Custom** | Any other pattern | Configurable per rule |

### Opt-Out Mechanisms

Modules using vertical slices or custom patterns can opt-out of layer enforcement:

**Via Assembly Attribute:**
```csharp
// In module's AssemblyInfo.cs or any file
[assembly: ModuleArchitecture(ModuleStyle.VerticalSlices)]
```

**Via .editorconfig:**
```ini
# Disable layer rules for a specific module
[src/Modules/SimpleModule/**/*.cs]
dotnet_diagnostic.MDYARCH003.severity = none
dotnet_diagnostic.MDYARCH004.severity = none
dotnet_diagnostic.MDYARCH005.severity = none
```

### Required Constraints (All Modules)

Regardless of internal architecture, ALL modules must:

1. **Expose registration via `IServiceCollection` extension method** - Enables composition at startup
2. **Not directly reference other modules' internal projects** - Only Contracts allowed
3. **Communicate cross-module only via Contracts** - Integration events and DTOs

These constraints are enforced by `MDYARCH001` (cross-module references) which cannot be disabled.

---

## Module Registration

Each module provides extension methods for DI registration:

```csharp
// Users.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}

// Users.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Database")));
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}

// Program.cs
builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration)
    .AddOrdersApplication()
    .AddOrdersInfrastructure(builder.Configuration);
```

## Related Documentation

- [Project Structure](project-structure.md)
- [SharedKernel Patterns](shared-kernel.md)
- [CQRS Patterns](cqrs-patterns.md)
