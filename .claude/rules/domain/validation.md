---
paths:
  - "**/*.cs"
---

<!-- ~700 tokens -->

# Validation Patterns

Validation happens at multiple layers with different responsibilities.

## Validation Layers

| Layer | Responsibility | Tool |
|-------|---------------|------|
| API/Presentation | Input format, required fields | FluentValidation |
| Application | Business rule preconditions | Pipeline behavior |
| Domain | Invariants, state transitions | Guard clauses in methods |

## FluentValidation (Input Validation)

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

## Pipeline Validation

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

## Domain Validation (Invariants)

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

## Value Object Validation

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

## Cross-Field Validation

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

## Async Validation (Uniqueness Checks)

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

## Prohibited

- Validation logic in controllers/endpoints
- Throwing exceptions for expected validation failures
- Mixing input validation with business rule validation
- Silent failures (swallowing validation errors)
- Duplicate validation across layers (validate once per concern)
