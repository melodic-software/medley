---
paths:
  - "**/*.cs"
---

<!-- ~450 tokens -->

# Dependency Injection Patterns

## Service Lifetimes

| Lifetime | Use For | Examples |
|----------|---------|----------|
| Singleton | Stateless, thread-safe services | `IConfiguration`, `IHttpClientFactory`, metrics |
| Scoped | Per-request/unit-of-work | `DbContext`, `IUserContext`, repositories |
| Transient | Lightweight, no shared state | Validators, handlers, factories |

## Registration Patterns

1. **Module extension methods** - Each module registers its own services
   ```csharp
   // In module
   public static class UsersModuleExtensions
   {
       public static IServiceCollection AddUsersModule(this IServiceCollection services)
       {
           services.AddScoped<IUserRepository, UserRepository>();
           services.AddScoped<IUserService, UserService>();
           return services;
       }
   }

   // In Program.cs
   builder.Services.AddUsersModule();
   builder.Services.AddOrdersModule();
   ```

2. **Options pattern** for configuration
   ```csharp
   services.Configure<EmailOptions>(configuration.GetSection("Email"));

   // Usage
   public class EmailService(IOptions<EmailOptions> options)
   {
       private readonly EmailOptions _options = options.Value;
   }
   ```

3. **Keyed services** (.NET 8+) for named registrations
   ```csharp
   services.AddKeyedSingleton<ICache, RedisCache>("distributed");
   services.AddKeyedSingleton<ICache, MemoryCache>("local");

   // Injection
   public class Service([FromKeyedServices("distributed")] ICache cache)
   ```

4. **Factory pattern** for runtime decisions
   ```csharp
   services.AddSingleton<IPaymentProcessorFactory, PaymentProcessorFactory>();
   services.AddTransient<StripeProcessor>();
   services.AddTransient<PayPalProcessor>();
   ```

## Primary Constructors

Use primary constructors for simple DI:

```csharp
// Good - concise
public class UserService(IUserRepository repository, ILogger<UserService> logger)
{
    public async Task<User?> GetAsync(UserId id) => await repository.GetByIdAsync(id);
}

// Avoid - verbose for simple cases
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

## Validation

Fail fast on missing dependencies:

```csharp
#if DEBUG
builder.Services.BuildServiceProvider().ValidateOnBuild();
#endif
```

## Prohibited

- **Service locator pattern** - Injecting `IServiceProvider` into business logic
  ```csharp
  // Bad - hides dependencies
  public class BadService(IServiceProvider provider)
  {
      public void DoWork() => provider.GetRequiredService<IDependency>().Execute();
  }
  ```

- **Captive dependencies** - Singleton holding Scoped service
  ```csharp
  // Bad - DbContext will be disposed while Singleton lives
  services.AddSingleton<BadService>();  // Injects scoped DbContext
  ```

- **Constructor over-injection** - More than 5-7 dependencies suggests SRP violation

- **Static service locator** - Never use static ServiceProvider access
