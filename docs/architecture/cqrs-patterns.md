# CQRS Patterns

Command Query Responsibility Segregation implementation using MediatR and FluentValidation.

## Overview

We use a **pragmatic approach** (2025 industry standard):
- MediatR for dispatching commands/queries
- FluentValidation inline with features
- Result pattern for explicit error handling

## Commands

Commands modify state and return `Result` or `Result<T>`:

```csharp
// Command definition
public record CreateUserCommand(string Email, string Name) : IRequest<Result<UserId>>;

// Handler
public class CreateUserHandler(
    IUserRepository repository,
    IValidator<CreateUserCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    public async Task<Result<UserId>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // Validate
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return Result.Failure<UserId>(validation.ToError());

        // Business logic
        var user = User.Create(cmd.Email, cmd.Name);
        repository.Add(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(user.Id);
    }
}

// Validator (FluentValidation)
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(IUserRepository repository)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) => !await repository.ExistsAsync(email, ct))
            .WithMessage("Email already exists");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

## Queries

Queries are read-only and return data directly (no Result wrapper for reads):

```csharp
// Query definition
public record GetUserQuery(Guid UserId) : IRequest<UserDto?>;

// Handler
public class GetUserHandler(IUserRepository repository)
    : IRequestHandler<GetUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(query.UserId, ct);
        return user?.ToDto();
    }
}
```

## Pipeline Behaviors

Cross-cutting concerns via MediatR pipeline:

### Validation Behavior

```csharp
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

### Unit of Work Behavior

```csharp
public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (IsQuery(request))
            return await next();

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var response = await next();
            await unitOfWork.CommitAsync(ct);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    private static bool IsQuery(TRequest request) =>
        request.GetType().Name.EndsWith("Query");
}
```

## Registration

```csharp
public static IServiceCollection AddCqrs(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
    });

    services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

    return services;
}
```

## API Integration

Map Results to HTTP responses:

```csharp
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        return result.Error switch
        {
            NotFoundError => TypedResults.NotFound(result.Error.ToProblemDetails()),
            ValidationError => TypedResults.BadRequest(result.Error.ToProblemDetails()),
            ConflictError => TypedResults.Conflict(result.Error.ToProblemDetails()),
            _ => TypedResults.Problem(result.Error.ToProblemDetails())
        };
    }

    private static ProblemDetails ToProblemDetails(this Error error) => new()
    {
        Title = error.Code,
        Detail = error.Message,
        Type = $"https://medley.app/errors/{error.Code.ToLowerInvariant()}"
    };
}

// Usage in endpoint
app.MapPost("/users", async (CreateUserCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.ToApiResult();
});
```

## Related Documentation

- [Project Structure](project-structure.md)
- [SharedKernel Patterns](shared-kernel.md)
- [Module Patterns](module-patterns.md)
