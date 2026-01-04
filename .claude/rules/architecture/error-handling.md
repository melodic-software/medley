---
paths:
  - "**/*.cs"
---

<!-- ~550 tokens -->

# Error Handling

Use Result<T> for expected failures, exceptions for unexpected failures.

## When to Throw vs Return Result<T>

| Scenario | Approach | Example |
|----------|----------|---------|
| Expected business failure | `Result<T>.Failure` | User not found, validation failed |
| Programmer error | Throw exception | Null argument, invalid state |
| External system failure | Throw, wrap at boundary | Database down, API timeout |
| Resource not found | `Result<T>.Failure` | Entity doesn't exist |

## Result<T> Pattern

Railway-oriented programming - chain operations that may fail:

```csharp
public async Task<Result<OrderDto>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
{
    return await _repository.GetByIdAsync(cmd.OrderId, ct)
        .ToResult(Error.NotFound("Order not found"))
        .Bind(order => order.Place())
        .Tap(order => _repository.Update(order))
        .TapAsync(_ => _unitOfWork.SaveChangesAsync(ct))
        .Map(order => order.ToDto());
}
```

## Error Types

Define domain-specific errors:

```csharp
public abstract record Error(string Code, string Message)
{
    public static Error NotFound(string message) => new NotFoundError(message);
    public static Error Validation(string message) => new ValidationError(message);
    public static Error Conflict(string message) => new ConflictError(message);
    public static Error Forbidden(string message) => new ForbiddenError(message);
}

public record NotFoundError(string Message) : Error("NOT_FOUND", Message);
public record ValidationError(string Message) : Error("VALIDATION_FAILED", Message);
public record ConflictError(string Message) : Error("CONFLICT", Message);
```

## API Error Responses (ProblemDetails)

Map domain errors to HTTP responses:

```csharp
public static IResult ToApiResponse<T>(this Result<T> result)
{
    return result.Match(
        success => TypedResults.Ok(success),
        error => error switch
        {
            NotFoundError => TypedResults.NotFound(ToProblemDetails(error)),
            ValidationError => TypedResults.BadRequest(ToProblemDetails(error)),
            ConflictError => TypedResults.Conflict(ToProblemDetails(error)),
            ForbiddenError => TypedResults.Forbid(),
            _ => TypedResults.Problem(ToProblemDetails(error))
        }
    );
}

private static ProblemDetails ToProblemDetails(Error error) => new()
{
    Title = error.Code,
    Detail = error.Message,
    Type = $"https://medley.app/errors/{error.Code.ToLowerInvariant()}"
};
```

## Exception Handling Middleware

Catch unhandled exceptions at the boundary:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred"
        });
    });
});
```

## Logging Errors

Log at appropriate levels:

```csharp
// Expected failures - Information or Warning
if (result.IsFailure)
    logger.LogWarning("Order placement failed: {Error}", result.Error.Message);

// Unexpected failures - Error
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error processing order {OrderId}", orderId);
    throw;
}
```

## Retry Patterns (External Failures)

Use Polly for transient failures:

```csharp
services.AddHttpClient<IExternalApi>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
```

## Prohibited

- Catching `Exception` without re-throwing (except at boundary)
- Empty catch blocks
- Using exceptions for control flow
- Returning null instead of Result<T> for failures
- Logging sensitive data in error messages
- Exposing stack traces to API clients
