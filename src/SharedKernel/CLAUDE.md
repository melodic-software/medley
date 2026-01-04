<!-- Last reviewed: 2026-01-04 -->
<!-- ~1,350 tokens -->
<!-- Lazy-loaded: Only included when working in src/SharedKernel/ or Application layer -->

# Application Layer Patterns

Context-specific guidance for Application layer development: CQRS handlers and validation.

---

## CQRS Patterns

Medley uses CQRS with project-owned abstractions wrapping MediatR.

### Command/Query Interfaces

```csharp
// Commands - modify state, return Result<T>
public interface ICommand<TResult> { }
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct);
}

// Queries - read-only, return Result<T>
public interface IQuery<TResult> { }
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct);
}
```

### Rules

1. **Use project interfaces, not MediatR directly** - Enables library swap without code changes

2. **Commands always return `Result<T>`** - Never throw for business failures
   ```csharp
   return user;           // Implicit success
   return Error.NotFound; // Implicit failure
   ```

3. **Queries are read-only** - No side effects, no state mutation

4. **One handler per command/query** - Single responsibility

5. **Vertical slice organization**:
   ```
   Features/
   └── Users/
       ├── CreateUser.cs      # Command + Handler + Validator
       ├── GetUserById.cs     # Query + Handler
       └── UserDto.cs         # Shared DTO
   ```

### Result Pattern Extensions (Railway-Oriented Programming)

```csharp
result.Map(x => transform(x))       // Transform success value (pure)
result.Bind(x => Validate(x))       // Chain operations that return Result<T>
result.Tap(x => sideEffect(x))      // Side effects on success
result.Match<U>(success, failure)   // Return value from either path

// Composition
var result = ValidateInput(input)
    .Bind(ValidateEmail)
    .Bind(ValidatePassword)
    .Map(CreateUser);

// Logical operators
result1 & result2  // Both must succeed
result1 | result2  // Either succeeds
```

**Map vs Bind**: Use `Map` for pure transforms; use `Bind` when the next step can fail.

### Pipeline Behaviors

Cross-cutting concerns via MediatR pipeline:

```csharp
// Validation behavior - runs before handler
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validator is null) return await next();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
            return (TResponse)TResponse.Failure(result.ToValidationErrors());

        return await next();
    }
}

// Logging behavior - wraps handler
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}
```

**Registration order**: Behaviors execute in registration order (first registered = outermost).

### Vertical Slice Organization

Medley uses a **hybrid approach**:
- **Module level**: Clean Architecture layers (Domain, Application, Infrastructure)
- **Within Application layer**: Vertical slices by feature

```
src/Modules/Orders/Orders.Application/
├── Features/
│   ├── PlaceOrder/
│   │   ├── PlaceOrderCommand.cs
│   │   ├── PlaceOrderHandler.cs
│   │   └── PlaceOrderValidator.cs
│   ├── GetOrderById/
│   │   └── GetOrderByIdQuery.cs    # Query + Handler in one file (simple)
│   └── CancelOrder/
│       ├── CancelOrderCommand.cs
│       └── CancelOrderHandler.cs
└── Common/
    └── OrderDto.cs
```

**File per slice**: Each slice is self-contained. Changes to one feature don't touch others.

### Prohibited - CQRS

- Using MediatR interfaces directly instead of project abstractions
- Throwing exceptions for business rule failures (use Result<T>)
- Commands with side effects in queries (violates CQS)
- Multiple handlers for the same command/query
- Leaking infrastructure concerns into handlers (DbContext in command handlers)

---

## Validation Patterns

Validation happens at multiple layers with different responsibilities.

### Validation Layers

| Layer | Responsibility | Tool |
|-------|---------------|------|
| API/Presentation | Input format, required fields | FluentValidation |
| Application | Business rule preconditions | Pipeline behavior |
| Domain | Invariants, state transitions | Guard clauses in methods |

### FluentValidation (Input Validation)

One validator per command/query. Validates input format and basic rules.

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain uppercase")
            .Matches("[0-9]").WithMessage("Must contain digit");
    }
}
```

### Pipeline Validation

Automatic validation via MediatR pipeline behavior:

```csharp
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validator is null)
            return await next();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage));
            return (TResponse)TResponse.ValidationFailure(errors);
        }

        return await next();
    }
}
```

**Result, not exceptions**: Return `Result<T>.ValidationFailure`, don't throw.

### Domain Validation (Invariants)

Protect aggregate invariants with guard clauses:

```csharp
public class Order : AggregateRoot<OrderId>
{
    public void Place()
    {
        // Guard clauses - domain invariants
        if (!Items.Any())
            return Error.Validation("Order must have at least one item");

        if (Status != OrderStatus.Draft)
            return Error.Validation("Only draft orders can be placed");

        Status = OrderStatus.Placed;
        AddDomainEvent(new OrderPlacedEvent(Id));
        return Result.Success();
    }
}
```

### Value Object Validation

Validate in constructor - invalid value objects can't exist:

```csharp
public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public static Result<Email> Create(string value)
    {
        try { return new Email(value); }
        catch (ArgumentException ex) { return Error.Validation(ex.Message); }
    }
}
```

**Factory method**: Use `Create()` for Result-based creation from user input.

### Cross-Field Validation

Complex rules involving multiple properties:

```csharp
public class DateRangeValidator : AbstractValidator<DateRangeQuery>
{
    public DateRangeValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).Days <= 365)
            .WithMessage("Date range cannot exceed one year");
    }
}
```

### Async Validation (Uniqueness Checks)

For database-dependent validation:

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Email)
            .MustAsync(async (email, ct) => !await repository.ExistsAsync(email, ct))
            .WithMessage("Email already registered");
    }
}
```

**Warning**: Async validators have race conditions. Handle duplicates at persistence layer too.

### Prohibited - Validation

- Validation logic in controllers/endpoints
- Throwing exceptions for expected validation failures
- Mixing input validation with business rule validation
- Silent failures (swallowing validation errors)
- Duplicate validation across layers (validate once per concern)
