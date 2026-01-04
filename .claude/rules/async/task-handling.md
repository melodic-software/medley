---
paths:
  - "**/*.cs"
---

<!-- ~600 tokens -->

# Async/Await Best Practices

## CancellationToken

1. **Accept in all async methods**
   ```csharp
   // Public APIs: use default for optional cancellation
   public async Task<User> GetUserAsync(UserId id, CancellationToken ct = default)

   // Internal methods called from ASP.NET Core: require the token
   internal async Task<User> GetUserInternalAsync(UserId id, CancellationToken ct)
   ```

2. **Pass to all downstream calls**
   ```csharp
   public async Task<Result<Order>> ProcessOrderAsync(OrderId id, CancellationToken ct)
   {
       var order = await _repository.GetByIdAsync(id, ct);
       await _paymentService.ChargeAsync(order.Total, ct);
       await _notificationService.SendAsync(order.CustomerId, ct);
       return order;
   }
   ```

3. **Check for cancellation in loops**
   ```csharp
   foreach (var item in items)
   {
       ct.ThrowIfCancellationRequested();
       await ProcessItemAsync(item, ct);
   }
   ```

## Task vs ValueTask

| Use `Task<T>` | Use `ValueTask<T>` |
|---------------|-------------------|
| Default choice | Hot paths with frequent sync completion |
| Always awaited once | Caching scenarios |
| Returned from interfaces | When benchmarks show improvement |

```csharp
// Good - ValueTask for cache hit path
public ValueTask<User?> GetUserAsync(UserId id, CancellationToken ct)
{
    if (_cache.TryGetValue(id, out var user))
        return ValueTask.FromResult(user);  // Sync path - no allocation

    return new ValueTask<User?>(GetFromDatabaseAsync(id, ct));
}
```

**Never await ValueTask multiple times** - undefined behavior.

## ConfigureAwait

| Context | ConfigureAwait(false) |
|---------|----------------------|
| ASP.NET Core handlers | Not needed (no SynchronizationContext) |
| SharedKernel / library code | Use `ConfigureAwait(false)` |
| Blazor Server components | Keep context for UI updates |
| Console apps | Not needed |

```csharp
// SharedKernel or library code that may be extracted
public async Task<Data> FetchAsync(CancellationToken ct)
{
    var response = await _client.GetAsync(url, ct).ConfigureAwait(false);
    return await response.Content.ReadFromJsonAsync<Data>(ct).ConfigureAwait(false);
}
```

**Note**: Code in `SharedKernel/` should always use `ConfigureAwait(false)` since it may be
extracted to a NuGet package or shared across different hosting contexts.

## Async Method Patterns

1. **Async suffix convention**
   ```csharp
   public Task<User> GetUserAsync(UserId id, CancellationToken ct)
   ```

2. **Return Task directly when possible** (avoids state machine)
   ```csharp
   // Good - no async/await overhead
   public Task<User> GetUserAsync(UserId id, CancellationToken ct)
       => _repository.GetByIdAsync(id, ct);

   // Use async/await only when needed
   public async Task<User> GetUserWithValidationAsync(UserId id, CancellationToken ct)
   {
       var user = await _repository.GetByIdAsync(id, ct);
       if (user is null) throw new NotFoundException();
       return user;
   }
   ```

3. **Async disposal**
   ```csharp
   await using var context = await _factory.CreateDbContextAsync(ct);
   ```

## Error Handling

```csharp
try
{
    await ProcessAsync(ct);
}
catch (OperationCanceledException) when (ct.IsCancellationRequested)
{
    _logger.LogInformation("Operation cancelled");
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}
```

## Prohibited

- **`.Result` or `.Wait()`** - Causes deadlocks
  ```csharp
  // Bad - deadlock risk
  var user = GetUserAsync(id).Result;
  ```

- **`async void`** - Except for event handlers
  ```csharp
  // Bad - exceptions are unobservable
  async void ProcessData() { }
  ```

- **Fire-and-forget without error handling**
  ```csharp
  // Bad - lost exceptions
  _ = SendEmailAsync(user);

  // Good - explicit background processing
  _ = Task.Run(async () =>
  {
      try { await SendEmailAsync(user); }
      catch (Exception ex) { _logger.LogError(ex, "Email failed"); }
  });
  ```

- **Mixing sync and async** in same operation
- **Ignoring CancellationToken** parameters
