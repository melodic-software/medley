---
paths:
  - "**/*.cs"
---

<!-- ~400 tokens -->

# Constants and Magic Strings

Avoid magic strings, numbers, and duplication. Use named constants, configuration, or enums to establish a single source of truth (SSOT) and follow DRY principles.

## When to Use Each Pattern

| Pattern | Use For | Example |
|---------|---------|---------|
| `const` | Compile-time primitives (numbers, strings, booleans) | HTTP status codes, regex patterns |
| `static readonly` | Runtime values, reference types, complex types | Collections, TimeSpan, calculated values |
| Configuration | Environment-specific or user-overridable values | Connection strings, API endpoints, feature flags |
| Enums | Related named integer values | OrderStatus, UserRole, LogLevel |
| `nameof()` | Type/member/property names | Reflection, property changed events |

## Const vs Static Readonly

```csharp
// const - inlined at compile time, must be primitive
public const int MaxRetries = 3;
public const string ApiVersion = "v2";

// static readonly - evaluated at runtime, can be any type
public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
public static readonly string[] AllowedExtensions = [".jpg", ".png", ".gif"];
public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
```

**Key difference**: `const` values are embedded in calling assemblies at compile time. Changing a `const` requires recompiling all dependent assemblies. Use `static readonly` for values that may change.

## Organization Patterns

### Feature-Scoped Constants (Preferred)

Group constants by feature/domain, not in global files:

```csharp
// Good - feature-scoped, discoverable
public static class OrderConstants
{
    public const int MaxItemsPerOrder = 100;
    public const decimal MinimumOrderValue = 10.00m;
    public static readonly TimeSpan ProcessingTimeout = TimeSpan.FromMinutes(30);
}

public static class UserRoles
{
    public const string Admin = "Administrator";
    public const string Manager = "Manager";
    public const string User = "User";
}
```

### Module-Level Constants

For module-wide values:

```csharp
// In Orders.Domain or Orders.Application
public static class OrderConfiguration
{
    public const string ModuleName = "Orders";
    public const string SchemaName = "Orders";
}
```

## Configuration Values

Use `appsettings.json` + Options pattern for environment-specific or user-overridable values:

```csharp
// appsettings.json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "FromAddress": "noreply@example.com"
  }
}

// Configuration class
public class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; } = string.Empty;
}

// Registration
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

// Usage
public class EmailService(IOptions<EmailOptions> options)
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        // Use _options.SmtpServer, etc.
    }
}
```

## nameof Operator

Use `nameof` for reflection-safe strings (avoids typos, supports refactoring):

```csharp
// Property names for validation
RuleFor(x => x.Email)
    .NotEmpty()
    .WithMessage($"{nameof(CreateUserCommand.Email)} is required");

// LINQ includes (refactor-safe)
context.Orders
    .Include(o => o.User)
    .Include(o => o.Items);  // Instead of .Include("Items")

// C# 14 - nameof with unbound generics
var typeName = nameof(List<>);  // Returns "List"
var genericName = nameof(Dictionary<,>);  // Returns "Dictionary"
```

## Reusable Functionality

### Extension Methods (Traditional)

For instance-like additions to existing types:

```csharp
public static class StringExtensions
{
    public static bool IsValidEmail(this string value)
        => !string.IsNullOrWhiteSpace(value) && value.Contains('@');
}

// Usage
if (email.IsValidEmail()) { }
```

### C# 14 Extension Members (Preferred)

For properties, operators, or static factories on types:

```csharp
// Extension block with instance and static members
public static class CollectionExtensions
{
    extension<T>(IEnumerable<T>)
    {
        // Instance-like extension
        public IEnumerable<T> Combine(IEnumerable<T> other) => this.Concat(other);

        // Static extension member
        public static IEnumerable<T> Empty => Enumerable.Empty<T>();
    }
}

// Usage
var combined = list1.Combine(list2);
var empty = IEnumerable<int>.Empty;
```

**When to use each**:
- Extension methods: Simple helpers, backward compatibility
- Extension members: Modern code, properties/operators, cleaner syntax

## Prohibited

- **Global Constants.cs god class** - Break into feature-scoped classes
  ```csharp
  // Bad - everything in one file
  public static class Constants
  {
      public const string AdminRole = "Admin";
      public const int MaxRetries = 3;
      public const string ApiEndpoint = "https://api.example.com";
      // ... 500 more constants
  }
  ```

- **Magic numbers in code**
  ```csharp
  // Bad
  if (order.Items.Count > 100) { }

  // Good
  if (order.Items.Count > OrderConstants.MaxItemsPerOrder) { }
  ```

- **String literals for property/type names**
  ```csharp
  // Bad
  .Include("Orders").ThenInclude("Items")

  // Good
  .Include(c => c.Orders).ThenInclude(o => o.Items)
  ```

- **Hardcoded configuration in code**
  ```csharp
  // Bad
  var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

  // Good
  services.AddHttpClient<IApiClient>(client =>
  {
      client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
  });
  ```

- **Duplicate functionality** - Extract to shared location
  ```csharp
  // Bad - duplicated across multiple classes
  private bool IsValidEmail(string email) => email.Contains('@');

  // Good - shared extension method or static utility
  public static class ValidationHelpers
  {
      public static bool IsValidEmail(string email)
          => !string.IsNullOrWhiteSpace(email) && email.Contains('@');
  }
  ```

- **Using `#region` for organization** - Use separate files/classes instead
