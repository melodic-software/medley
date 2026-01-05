<!-- Last reviewed: 2026-01-04 -->
<!-- ~1,100 tokens -->
<!-- Lazy-loaded: Only included when working in tests/ directory -->

# Testing Conventions

## Project Structure

```
tests/
├── Medley.UnitTests/           # Fast, isolated tests
├── Medley.IntegrationTests/    # Database, API tests
├── Medley.ArchitectureTests/   # ArchUnitNET / NetArchTest
└── Medley.EndToEndTests/       # Playwright / full stack
```

## Naming Convention

```csharp
// Class: {SystemUnderTest}Tests
public class CreateUserCommandHandlerTests

// Method: {Method}_{Scenario}_{ExpectedResult}
public async Task HandleAsync_ValidUser_ReturnsSuccess()
public async Task HandleAsync_DuplicateEmail_ReturnsValidationError()
```

## AAA Pattern

```csharp
[Fact]
public async Task HandleAsync_ValidUser_ReturnsSuccess()
{
    // Arrange
    var command = new CreateUserCommand("test@example.com", "Test User");
    var handler = new CreateUserCommandHandler(_repository, _validator);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.Email.ShouldBe("test@example.com");
}
```

## Framework

Use **xUnit v3** with **Shouldly** for assertions.

```xml
<!-- Required packages -->
<PackageReference Include="xunit.v3" Version="3.*" />
<PackageReference Include="xunit.v3.runner.visualstudio" Version="3.*" />
<PackageReference Include="Shouldly" Version="4.*" />

<!-- xUnit v3 requires Exe output type -->
<OutputType>Exe</OutputType>
```

Shouldly is free/open-source with no licensing concerns (unlike FluentAssertions v8+
which requires paid commercial license).

```csharp
// Shouldly - natural English-like syntax
result.ShouldBe(expected);
result.ShouldBeOfType<User>();
collection.ShouldContain(x => x.Email == "test@example.com");
collection.ShouldBeEmpty();

// Exception assertions - capture for further assertions
var ex = Should.Throw<ValidationException>(() => handler.Handle(invalidCommand));
ex.Message.ShouldContain("email");
ex.Errors.ShouldContain(e => e.PropertyName == "Email");

await Should.ThrowAsync<NotFoundException>(async () => await handler.HandleAsync(query));

// Result<T> assertions
result.IsSuccess.ShouldBeTrue();
result.Error.Code.ShouldBe("USER_NOT_FOUND");
```

## xUnit v3 Patterns

### Async Setup/Teardown (IAsyncLifetime)

Use `IAsyncLifetime` for async test initialization:

```csharp
public class DatabaseTests : IAsyncLifetime
{
    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _context = await CreateTestContextAsync();
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task Query_ReturnsExpectedData()
    {
        var result = await _context.Users.ToListAsync();
        result.ShouldNotBeEmpty();
    }
}
```

### Parameterized Tests (TheoryDataRow)

Use `TheoryDataRow` for typed parameterized tests:

```csharp
public static TheoryData<string, bool> EmailValidationData => new()
{
    { "valid@example.com", true },
    { "invalid-email", false },
    { "", false },
};

[Theory]
[MemberData(nameof(EmailValidationData))]
public void ValidateEmail_ReturnsExpected(string email, bool expected)
{
    var result = EmailValidator.IsValid(email);
    result.ShouldBe(expected);
}
```

### Dynamic Skip (Conditional Tests)

Skip tests based on runtime conditions:

```csharp
[Fact]
[SkipWhen(nameof(IsRunningInCI), typeof(ConditionalTests))]
public void Test_RequiresLocalResources()
{
    // Skipped when running in CI
}

public static bool IsRunningInCI =>
    Environment.GetEnvironmentVariable("CI") == "true";
```

## Rules

1. **One logical assertion per test** - Keep tests focused

2. **Use descriptive test data**
   ```csharp
   // Good
   var email = "valid.user@example.com";

   // Bad
   var email = "a@b.c";
   ```

3. **Isolate unit tests** - Mock external dependencies

4. **Integration tests use WebApplicationFactory**
   ```csharp
   public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
   ```

5. **Test Result<T> explicitly**
   ```csharp
   result.IsSuccess.ShouldBeTrue();
   result.IsFailure.ShouldBeTrue();
   result.Error.Code.ShouldBe("USER_NOT_FOUND");
   ```

## Integration Testing Patterns

1. **Custom WebApplicationFactory** for test configuration
   ```csharp
   public class TestWebApplicationFactory : WebApplicationFactory<Program>
   {
       protected override void ConfigureWebHost(IWebHostBuilder builder)
       {
           builder.ConfigureServices(services =>
           {
               // Replace DbContext with test database
               services.RemoveAll<DbContextOptions<AppDbContext>>();
               services.AddDbContext<AppDbContext>(options =>
                   options.UseInMemoryDatabase("TestDb"));
           });
       }
   }
   ```

2. **Mock authentication** for authorized endpoints
   ```csharp
   builder.ConfigureTestServices(services =>
   {
       services.AddAuthentication("Test")
           .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", null);
   });
   ```

3. **Test data builders** for readable test setup
   ```csharp
   var user = new UserBuilder()
       .WithEmail("test@example.com")
       .WithRole("Admin")
       .Build();
   ```

4. **SQLite in-memory** for realistic database testing
   ```csharp
   var connection = new SqliteConnection("DataSource=:memory:");
   connection.Open();
   services.AddDbContext<AppDbContext>(options =>
       options.UseSqlite(connection));
   ```

## Documentation

Test code is exempt from XML documentation requirements:

- Test method names ARE the documentation (`MethodName_Scenario_ExpectedBehavior`)
- No XML comments on test classes, methods, or fixtures
- Comments within tests only for complex setup explanation

This aligns with the project's Clean Code philosophy - tests document behavior through naming.

## Prohibited

- Tests that depend on execution order
- Shared mutable state between tests
- Testing implementation details instead of behavior
- Hard-coded connection strings in tests
- `Thread.Sleep` or arbitrary delays (use async patterns)
- FluentAssertions (licensing restrictions in v8+, use Shouldly)
- Synchronous setup in async tests (use `IAsyncLifetime`)
