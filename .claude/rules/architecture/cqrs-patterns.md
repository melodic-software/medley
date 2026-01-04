---
paths:
  - "src/**/*.cs"
---

<!-- ~550 tokens -->

# CQRS Patterns

Medley uses CQRS with project-owned abstractions wrapping MediatR.

## Command/Query Interfaces

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

## Rules

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

## Result Pattern Extensions (Railway-Oriented Programming)

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

## Pipeline Behaviors

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

## Vertical Slice Organization

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

## Prohibited

- Using MediatR interfaces directly instead of project abstractions
- Throwing exceptions for business rule failures (use Result<T>)
- Commands with side effects in queries (violates CQS)
- Multiple handlers for the same command/query
- Leaking infrastructure concerns into handlers (DbContext in command handlers)
