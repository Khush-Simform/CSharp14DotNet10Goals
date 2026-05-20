# Prompt Engineering Comparison — REST API Endpoint

**Task:** Add a "deactivate client" endpoint to the CA Management API.

---

## Round 1 — Vague Prompt

> **Prompt given to Copilot:**
> ```
> add an endpoint to deactivate a client
> ```

### What Copilot Typically Produces

```csharp
// ClientsController.cs — Copilot inline suggestion
[HttpPatch("{id}/deactivate")]
public IActionResult Deactivate(Guid id)
{
    var client = _db.Clients.Find(id);
    client.IsActive = false;
    _db.SaveChanges();
    return Ok();
}
```

### Problems with This Output

| Problem | Impact |
|---|---|
| Assumes `_db` (EF Core) — this project uses an in-memory `CaDataStore` | Won't compile at all |
| No null-check before `client.IsActive = false` | NullReferenceException on unknown IDs |
| No 404 response when client doesn't exist | Wrong HTTP contract |
| No guard for already-inactive clients | Silently no-ops instead of signaling conflict |
| Returns `Ok()` with no body | Caller cannot confirm the new state |
| No `CancellationToken` parameter | Ignores cooperative cancellation |
| No `[SwaggerOperation]` / `[ProducesResponseType]` | Endpoint invisible in Swagger UI |
| Bypasses the service layer (`IClientService`) | Breaks separation of concerns, untestable |
| `IsActive` is `init`-only on `Client` | Won't compile — property is read-only after construction |

**Overall:** The suggestion has 9 distinct defects. It cannot compile, has wrong HTTP semantics, and violates every architectural pattern already established in the project.

---

## Round 2 — Structured Prompt

> **Prompt given to Copilot:**
> ```
> Add PATCH /api/clients/{id:guid}/deactivate to this ASP.NET Core Minimal + Controller API.
>
> Constraints and expected behaviour:
> - Route: PATCH /api/clients/{id:guid}/deactivate
> - 404 ProblemDetails  → client ID not found  (use existing NotFoundException)
> - 409 ProblemDetails  → client is already inactive (create a new ConflictException,
>   add it to ExceptionHandlingMiddleware alongside NotFoundException)
> - 200 ClientResponse  → success; return the updated record
> - Follow the existing service-layer pattern: add DeactivateAsync(Guid, CancellationToken)
>   to IClientService, implement it in ClientService using CaDataStore.Query/Mutate
> - Client.IsActive is currently init-only — change it to { get; set; } so the store can flip it
> - Annotate the controller action with [SwaggerOperation] + all [ProducesResponseType] attributes
>   matching the existing controller style (see ClientsController.cs)
> - Accept CancellationToken and call ThrowIfCancellationRequested() at the top of the service method
> ```

### What the Structured Prompt Produced (Implemented in This PR)

**New / changed files:**

| File | Change |
|---|---|
| `Models/Client.cs` | `IsActive` changed from `init` → `set` |
| `Services/Exceptions/ConflictException.cs` | New — typed 409 exception |
| `Services/IClientService.cs` | `DeactivateAsync(Guid, CancellationToken)` added |
| `Services/ClientService.cs` | Full implementation with 404 + 409 guards |
| `Middlewares/ExceptionHandlingMiddleware.cs` | `ConflictException` → 409 mapping added |
| `Controllers/ClientsController.cs` | `DeactivateAsync` action with full OpenAPI annotations |

**ClientService.DeactivateAsync (structured result):**

```csharp
public Task<ClientResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    var client = store.Query(s => s.Clients.FirstOrDefault(c => c.Id == id))
        ?? throw new NotFoundException($"Client '{id}' was not found.");

    if (!client.IsActive)
        throw new ConflictException($"Client '{id}' is already inactive.");

    store.Mutate(s =>
    {
        var target = s.Clients.First(c => c.Id == id);
        target.IsActive = false;
    });

    var updated = store.Query(s => s.Clients.First(c => c.Id == id));
    return Task.FromResult(updated.ToResponse());
}
```

**ClientsController action (structured result):**

```csharp
/// <summary>Marks a client as inactive. Cannot be reversed via this API.</summary>
[HttpPatch("{id:guid}/deactivate")]
[SwaggerOperation(
    Summary = "Deactivate a client",
    Description = "Sets the client's IsActive flag to false. Returns 404 if the client does not exist, 409 if already inactive.")]
[ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<ActionResult<ClientResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken)
{
    var result = await clients.DeactivateAsync(id, cancellationToken);
    return Ok(result);
}
```

---

## Comparison Summary

| Dimension | Vague Prompt | Structured Prompt |
|---|---|---|
| **Compiles** | No (wrong DB abstraction, init-only property) | Yes — 0 errors, 0 warnings |
| **HTTP contract** | Returns `200 Ok()` (no body) | Returns `200 ClientResponse`, `404`, `409` |
| **Error handling** | None | NotFoundException + ConflictException, both mapped to RFC 7807 ProblemDetails |
| **Architecture** | Bypasses service layer | Follows IClientService → ClientService pattern |
| **Thread safety** | Accesses data directly | Uses CaDataStore.Query / Mutate (lock-protected) |
| **Observability** | No OpenAPI docs | Full SwaggerOperation + ProducesResponseType coverage |
| **Cancellation** | Missing | CancellationToken propagated and checked |
| **Idempotency** | Silent no-op | 409 Conflict signals caller that state already matches |
| **Lines changed** | ~7 (wrong) | ~50 across 6 files (correct) |

---

## Key Lessons

1. **Context is the prompt.** Copilot's suggestions are only as good as the patterns it can see. Naming the existing types (`CaDataStore`, `NotFoundException`, `ExceptionHandlingMiddleware`) in the prompt pulls in the right context rather than letting the model hallucinate a generic EF Core pattern.

2. **Specify HTTP semantics explicitly.** "Deactivate a client" says nothing about what should happen when the ID doesn't exist or when the client is already inactive. Callers of an API need predictable status codes.

3. **Call out cross-cutting concerns.** Validation, error handling, OpenAPI docs, and cancellation are invisible to a vague prompt. A structured prompt names them and gets them included.

4. **Point at existing constraints.** The `init`-only property would silently break a vague suggestion. Mentioning it upfront shifts the solution from wrong to correct.

5. **Structured prompts pay compound interest.** Each extra sentence in a structured prompt eliminates an entire category of follow-up fixes. The vague prompt produced 9 defects that would each require a separate correction round.
