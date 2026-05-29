---
name: scaffold-ca-endpoint
description: Use this skill when asked to add a new entity or domain object to the CA Management API. Scaffolds the Model, Request/Response DTOs, Service interface, Service implementation, Controller, DtoMapper extension methods, DI registration, and optional seed data — all following this project's established patterns.
---

Scaffold a complete, production-ready CA Management API endpoint for a new entity by following every step below in order. Consult `templates/patterns-reference.md` in this skill's directory for exact code shapes, then run `scripts/verify-scaffold.sh` at the end to confirm all files are in place.

---

## Step 0 — Gather requirements

Before writing any code, confirm:
- **Entity name** (singular PascalCase, e.g. `Invoice`)
- **Properties** (name, type, required/optional, any validation constraints)
- **Relationships** (does it belong to a `Client`? does it reference an `Account`?)
- **Operations needed** (Create, GetById, GetAll, Update, Delete, or a subset)

If a property requires domain-specific validation (PAN, IBAN, email format), note the regex pattern now.

---

## Step 1 — Create the Model

File: `Models/{EntityName}.cs`

Rules:
- `public sealed class {EntityName}` — always `sealed`
- All properties use `{ get; init; }` — immutable after construction
- Required value-type properties use `required`; nullable reference types use `?` and are optional
- Use `Guid` for `Id`; timestamps are `DateTimeOffset` (never `DateTime`); dates only are `DateOnly`
- Enum navigation properties use the enum type (e.g. `AccountCategory`), never a raw `string`
- Namespace: `CaManagement.Api.Models`
- No data-annotation attributes on models — annotations belong on DTOs only

---

## Step 2 — Create enums (if needed)

File: `Models/{EnumName}.cs`

Rules:
- One public enum per file
- Use `None = 0` as the default value only if "not set" is a valid state; otherwise start at `1`
- Namespace: `CaManagement.Api.Models`

---

## Step 3 — Create the Response DTO

File: `DTOs/{EntityNamePlural}/{EntityName}Response.cs`

Rules:
- `public sealed class {EntityName}Response` — always `sealed`
- All properties `{ get; init; }` and `required`
- Enum properties are surfaced as `string` (Copilot will call `.ToString()` in the mapper)
- Computed or derived fields (e.g. `ComputedBalance`) belong here, not on the model
- Namespace: `CaManagement.Api.DTOs.{EntityNamePlural}`

---

## Step 4 — Create the Request DTO(s)

File: `DTOs/{EntityNamePlural}/Create{EntityName}Request.cs` (and `Update{EntityName}Request.cs` if update is in scope)

Rules:
- `public sealed class Create{EntityName}Request`
- Use `[Required]`, `[MinLength]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]` from `System.ComponentModel.DataAnnotations` — no FluentValidation
- `string` properties that are optional have no `[Required]` and are declared as `string?`
- Namespace: `CaManagement.Api.DTOs.{EntityNamePlural}`

---

## Step 5 — Add DtoMapper extension methods

File: `Helpers/CaDtoMapper.cs` — **append** to the existing static class; do NOT recreate the file.

Add two extension methods:
1. `public static {EntityName}Response ToResponse(this {EntityName} e)` — maps model → response DTO
2. `public static {EntityName} ToDomain(this Create{EntityName}Request dto, Guid id, ...)` — maps request DTO → model

Rules:
- Trim whitespace on all `string` inputs: `dto.Name.Trim()`
- For optional strings: `string.IsNullOrWhiteSpace(dto.X) ? null : dto.X.Trim()`
- Normalize identifiers to upper-case where appropriate (e.g. PAN codes)
- Enum properties: assign directly from the DTO (enum is already the correct type after model binding)
- Add any new `using` directives for new DTO namespaces at the top of the file

---

## Step 6 — Create the Service interface

File: `Services/I{EntityName}Service.cs`

Rules:
- `public interface I{EntityName}Service`
- One method per operation; return types use Response DTOs (never models)
- Use `IReadOnlyList<{EntityName}Response>` for collections
- No `async` — this project uses synchronous in-memory storage
- Namespace: `CaManagement.Api.Services`

---

## Step 7 — Create the Service implementation

File: `Services/{EntityName}Service.cs`

Rules:
- `public sealed class {EntityName}Service(CaDataStore store, TimeProvider time) : I{EntityName}Service`
- Primary constructor injection — no field declarations, no constructor body
- Access the data store via `store.BeginTransaction()` / `store.Commit()` / `store.Rollback()` for writes; read operations use `store.Snapshot()`
- Generate new IDs with `Guid.NewGuid()`
- Timestamps via `time.GetUtcNow()`
- Throw `NotFoundException` (from `Services/Exceptions/`) when an entity is not found
- Throw `ConflictException` when a uniqueness rule is violated
- Throw `DomainValidationException` for business-rule violations
- Call `dto.ToDomain(...)` and `.ToResponse()` from `CaDtoMapper`
- Namespace: `CaManagement.Api.Services`

---

## Step 8 — Create the Controller

File: `Controllers/{EntityName}sController.cs`

Rules:
- `[ApiController]`, `[Route("api/[controller]")]`
- `public sealed class {EntityName}sController(I{EntityName}Service svc) : ControllerBase`
- Return `Ok(result)` for success; `NotFound()` / `Conflict()` / `BadRequest()` for errors (the `ExceptionHandlingMiddleware` will catch domain exceptions automatically, so you only need explicit error returns when the service returns `null` or a boolean)
- Use `[ProducesResponseType]` attributes to document status codes
- Use `[FromBody]` for POST/PUT bodies; `[FromRoute]` for route parameters
- Namespace: `CaManagement.Api.Controllers`

---

## Step 9 — Register the service in DI

File: `Configurations/ServiceCollectionExtensions.cs` — **append** one line to the existing method body:

```csharp
services.AddScoped<I{EntityName}Service, {EntityName}Service>();
```

Add it after the last existing `AddScoped` call. Also add the required `using` for `CaManagement.Api.Services` if it is not already present (it should be).

---

## Step 10 — Add seed data (optional but recommended)

File: `Helpers/SeedData.cs` — **append** seed entries for the new entity so the API is immediately explorable without manual setup. Mirror the style of existing seed entries.

---

## Step 11 — Verify

Run the verification script from the repository root:

```bash
bash .github/skills/scaffold-ca-endpoint/scripts/verify-scaffold.sh {EntityName}
```

Fix any issues the script reports before committing.

---

## Quality checklist (review before opening a PR)

- [ ] `sealed` on every new class
- [ ] `init`-only properties on all models and DTOs
- [ ] All string inputs trimmed in `ToDomain`
- [ ] Data annotations present on every required request-DTO property
- [ ] Service registered in `ServiceCollectionExtensions.cs`
- [ ] DtoMapper has both `ToResponse` and `ToDomain` for the new entity
- [ ] No `DateTime` used anywhere — use `DateTimeOffset` or `DateOnly`
- [ ] No `async`/`await` — the data store is synchronous
