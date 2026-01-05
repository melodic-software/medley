---
paths:
  - "**/*.cs"
---

<!-- ~650 tokens -->

# Code Style Fundamentals

Key conventions reinforced from `.editorconfig`. Full rules enforced by analyzers.

## File Organization

1. **File-scoped namespaces** (IDE0161)
   ```csharp
   namespace Medley.Users.Application;

   public class CreateUserCommand { }
   ```

2. **Using directives** - Outside namespace, sorted, System first

3. **One type per file** - File name matches type name

## Naming

| Element | Style | Example |
|---------|-------|---------|
| Class, Record, Struct | PascalCase | `UserService` |
| Interface | IPascalCase | `IUserRepository` |
| Method | PascalCase | `GetUserAsync` |
| Property | PascalCase | `EmailAddress` |
| Private field | _camelCase | `_userRepository` |
| Parameter | camelCase | `userId` |
| Local | camelCase | `userCount` |
| Constant | PascalCase | `MaxRetryCount` |

### Type Suffix Conventions

Enforced by Roslyn analyzers (MDYNAME rules):

| Type | Required Suffix | Example |
|------|-----------------|---------|
| Repository implementations | `Repository` | `UserRepository` |
| FluentValidation validators | `Validator` | `CreateUserValidator` |
| MediatR handlers | `Handler` | `CreateUserHandler` |
| Specification classes | `Specification` | `ActiveUsersSpecification` |
| EF Core configurations | `Configuration` | `UserConfiguration` |
| Domain/Application services | `Service` | `EmailService` |
| DTOs | `Dto` | `UserDto` |

**Note**: Handler uses simple `Handler` suffix (not `CommandHandler`). Namespace provides context.

## C# 14 Features (Use When Appropriate)

All features below are stable and released with .NET 10 (November 2025).

- **Primary constructors** for simple DI
- **Collection expressions** `[1, 2, 3]`
- **Pattern matching** for type checks
- **Records** for immutable DTOs
- **Required members** for mandatory properties
- **`field` keyword** for property backing fields (reduces boilerplate)
  ```csharp
  public string Name
  {
      get => field;
      set => field = value ?? throw new ArgumentNullException();
  }
  ```
- **Extension members** for extending types with properties/static methods
  ```csharp
  public static class UserExtensions
  {
      extension(User user)
      {
          public string DisplayName => $"{user.FirstName} {user.LastName}";
      }

      extension(User)  // Static extension members
      {
          public static User Anonymous => new("anonymous@example.com");
      }
  }
  ```
- **Null-conditional assignment** for conditional property setting
  ```csharp
  customer?.Order = GetDefaultOrder();  // Only assigns if customer not null
  ```
- **`nameof` with unbound generics** for cleaner reflection code
  ```csharp
  var typeName = nameof(List<>);  // Returns "List"
  ```
- **First-class Span<T>** with implicit conversions for high-performance
  ```csharp
  void ProcessData(ReadOnlySpan<byte> data) { /* ... */ }
  byte[] bytes = GetBytes();
  ProcessData(bytes);  // Implicit conversion to ReadOnlySpan<byte>
  ```
- **Lambda parameter modifiers** (`ref`, `in`, `out`) without explicit types
  ```csharp
  var increment = (ref int x) => x++;
  var tryParse = (string s, out int result) => int.TryParse(s, out result);
  ```
- **Partial events and constructors** for splitting across files
  ```csharp
  partial class UserService
  {
      public partial event EventHandler<UserEventArgs>? UserCreated;
      public partial UserService(ILogger logger);
  }
  ```
- **User-defined compound operators** for custom types (C# 14 instance methods)
  ```csharp
  public readonly struct Money(decimal amount)
  {
      public decimal Amount { get; private set; } = amount;

      // Traditional operator returns new instance
      public static Money operator +(Money left, Money right)
          => new(left.Amount + right.Amount);

      // Compound assignment mutates in-place (void return, instance method)
      public void operator +=(Money right) => Amount += right.Amount;
  }
  ```

## Formatting

- **Indentation**: 4 spaces (no tabs)
- **Braces**: New line (Allman style)
- **Max line length**: 120 characters
- **Trailing newline**: Required

## Expression Preferences

```csharp
// Prefer expression body for simple members
public string FullName => $"{FirstName} {LastName}";

// Prefer block body for complex logic
public Result<User> CreateUser(CreateUserCommand command)
{
    // Multiple statements
}
```

## Null Handling

- **Nullable reference types**: Enabled project-wide
- **Use null-conditional**: `user?.Email`
- **Use null-coalescing**: `value ?? defaultValue`
- **Avoid null checks where Result<T> applies**

## Prohibited

- Block-scoped namespaces (use file-scoped)
- Multiple types per file (except nested types)
- Tabs for indentation (use 4 spaces)
- Hungarian notation or type prefixes
- Abbreviations in public APIs (except well-known: Id, Url, Html)
- Disabling nullable reference types (`#nullable disable`)
