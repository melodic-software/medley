# SharedKernel Patterns

Cross-cutting DDD building blocks shared across all modules.

## Overview

SharedKernel follows industry-standard patterns from Ardalis Clean Architecture and DDD best practices. It provides reusable abstractions that can be extracted to NuGet packages.

## Project Structure

```
SharedKernel/
├── SharedKernel/                 # Core DDD primitives (ZERO external deps)
├── SharedKernel.Application/     # CQRS abstractions (MediatR)
├── SharedKernel.Infrastructure/  # EF Core implementations
├── SharedKernel.Contracts/       # Cross-module communication
└── SharedKernel.Analyzers/       # Roslyn analyzers (future)
```

## SharedKernel (Core)

**Dependencies**: ZERO external NuGet packages (only .NET BCL)

```
SharedKernel/
├── Entities/
│   ├── IEntity.cs               # Marker interface
│   ├── Entity{TId}.cs           # Base class with strongly-typed Id
│   └── IAuditableEntity.cs      # DateCreated, CreatedBy, etc.
├── Aggregates/
│   ├── IAggregateRoot.cs        # Marker (extends IEntity)
│   └── AggregateRoot{TId}.cs    # Base class with domain events
├── ValueObjects/
│   └── ValueObject.cs           # Abstract base with equality
├── Events/
│   ├── IDomainEvent.cs          # Marker interface
│   └── DomainEvent.cs           # Base class with timestamp
├── Results/
│   ├── Result.cs                # Success/Failure without value
│   ├── Result{T}.cs             # Success with value or Error
│   └── Error.cs                 # Error types (NotFound, Validation, etc.)
├── Specifications/
│   └── ISpecification{T}.cs     # Query specification pattern
└── Repositories/
    └── IRepository{T}.cs        # Generic repository interface
```

### Result Pattern

Railway-oriented programming for explicit error handling:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public abstract record Error(string Code, string Message)
{
    public static readonly Error None = new NoneError();
    public static Error NotFound(string message) => new NotFoundError(message);
    public static Error Validation(string message) => new ValidationError(message);
    public static Error Conflict(string message) => new ConflictError(message);
}
```

## SharedKernel.Application

**Dependencies**: MediatR, references SharedKernel

```
SharedKernel.Application/
├── Commands/
│   ├── ICommand.cs              # : IRequest<Result> (MediatR)
│   └── ICommand{TResult}.cs     # : IRequest<Result<TResult>>
├── Queries/
│   └── IQuery{TResult}.cs       # : IRequest<TResult>
├── Behaviors/
│   ├── ValidationBehavior.cs    # FluentValidation pipeline
│   ├── LoggingBehavior.cs       # Request/response logging
│   └── UnitOfWorkBehavior.cs    # Transaction handling
└── Abstractions/
    ├── IUnitOfWork.cs           # Transaction abstraction
    └── IDateTimeProvider.cs     # Testable time abstraction
```

## SharedKernel.Infrastructure

**Dependencies**: EF Core, MediatR, references SharedKernel.Application

```
SharedKernel.Infrastructure/
├── Persistence/
│   ├── BaseDbContext.cs         # Common DbContext config
│   ├── Repository{T}.cs         # Generic repository implementation
│   └── Interceptors/
│       ├── AuditableEntityInterceptor.cs
│       ├── DomainEventDispatcherInterceptor.cs
│       └── SoftDeleteInterceptor.cs
├── EventDispatching/
│   └── DomainEventDispatcher.cs # Dispatches events via MediatR
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## SharedKernel.Contracts

**Dependencies**: References SharedKernel only

```
SharedKernel.Contracts/
├── IntegrationEvents/
│   ├── IIntegrationEvent.cs     # Marker for cross-module events
│   └── IntegrationEvent.cs      # Base with correlation, timestamp
└── Dtos/
    └── PagedResult{T}.cs        # Pagination DTO
```

## Design Principles

1. **Core has ZERO external dependencies** - Can be used anywhere
2. **Application layer uses MediatR** - Pragmatic CQRS implementation
3. **Infrastructure implements abstractions** - Dependency inversion
4. **Contracts for cross-module communication** - Module isolation

## Related Documentation

- [Project Structure](project-structure.md)
- [Module Patterns](module-patterns.md)
- [CQRS Patterns](cqrs-patterns.md)
