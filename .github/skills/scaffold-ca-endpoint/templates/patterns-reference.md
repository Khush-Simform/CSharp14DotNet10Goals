# CA Management API — Code Patterns Reference

This file contains exact code shapes extracted from the live codebase. Use these as the authoritative templates when scaffolding a new entity. The placeholder entity used throughout is `Invoice`.

---

## Model (`Models/Invoice.cs`)

```csharp
namespace CaManagement.Api.Models;

public sealed class Invoice
{
    public required Guid        Id           { get; init; }
    public required Guid        ClientId     { get; init; }
    public required string      InvoiceNumber { get; init; }
    public required decimal     Amount       { get; init; }
    public required DateOnly    IssuedOn     { get; init; }
    public          DateOnly?   DueOn        { get; init; }
    public required InvoiceStatus Status     { get; init; }   // enum, not string
    public          string?     Notes        { get; init; }
}
```

---

## Enum (`Models/InvoiceStatus.cs`)

```csharp
namespace CaManagement.Api.Models;

public enum InvoiceStatus
{
    Draft   = 1,
    Issued  = 2,
    Paid    = 3,
    Overdue = 4,
    Voided  = 5,
}
```

---

## Response DTO (`DTOs/Invoices/InvoiceResponse.cs`)

```csharp
namespace CaManagement.Api.DTOs.Invoices;

public sealed class InvoiceResponse
{
    public required Guid     Id            { get; init; }
    public required Guid     ClientId      { get; init; }
    public required string   InvoiceNumber { get; init; }
    public required decimal  Amount        { get; init; }
    public required DateOnly IssuedOn      { get; init; }
    public          DateOnly? DueOn        { get; init; }
    public required string   Status        { get; init; }  // enum surfaced as string
    public          string?  Notes         { get; init; }
}
```

---

## Request DTO (`DTOs/Invoices/CreateInvoiceRequest.cs`)

```csharp
using System.ComponentModel.DataAnnotations;

namespace CaManagement.Api.DTOs.Invoices;

public sealed class CreateInvoiceRequest
{
    [Required]
    public Guid ClientId { get; init; }

    [Required]
    [MinLength(3)]
    [MaxLength(30)]
    public string InvoiceNumber { get; init; } = string.Empty;

    [Required]
    [Range(0.01, 10_000_000)]
    public decimal Amount { get; init; }

    [Required]
    public DateOnly IssuedOn { get; init; }

    public DateOnly? DueOn { get; init; }

    [Required]
    public InvoiceStatus Status { get; init; }

    [MaxLength(500)]
    public string? Notes { get; init; }
}
```

---

## DtoMapper entries (append to `Helpers/CaDtoMapper.cs`)

```csharp
// ── Invoice ─────────────────────────────────────────────────────────────────

public static InvoiceResponse ToResponse(this Invoice inv) => new()
{
    Id            = inv.Id,
    ClientId      = inv.ClientId,
    InvoiceNumber = inv.InvoiceNumber,
    Amount        = inv.Amount,
    IssuedOn      = inv.IssuedOn,
    DueOn         = inv.DueOn,
    Status        = inv.Status.ToString(),
    Notes         = inv.Notes,
};

public static Invoice ToDomain(this CreateInvoiceRequest dto, Guid id) => new()
{
    Id            = id,
    ClientId      = dto.ClientId,
    InvoiceNumber = dto.InvoiceNumber.Trim().ToUpperInvariant(),
    Amount        = dto.Amount,
    IssuedOn      = dto.IssuedOn,
    DueOn         = dto.DueOn,
    Status        = dto.Status,
    Notes         = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
};
```

---

## Service interface (`Services/IInvoiceService.cs`)

```csharp
using CaManagement.Api.DTOs.Invoices;

namespace CaManagement.Api.Services;

public interface IInvoiceService
{
    IReadOnlyList<InvoiceResponse> GetAll(Guid clientId);
    InvoiceResponse                GetById(Guid id);
    InvoiceResponse                Create(CreateInvoiceRequest request);
    void                           Delete(Guid id);
}
```

---

## Service implementation (`Services/InvoiceService.cs`)

```csharp
using CaManagement.Api.DTOs.Invoices;
using CaManagement.Api.Helpers;
using CaManagement.Api.Services.Exceptions;

namespace CaManagement.Api.Services;

public sealed class InvoiceService(CaDataStore store, TimeProvider time) : IInvoiceService
{
    public IReadOnlyList<InvoiceResponse> GetAll(Guid clientId)
    {
        var snapshot = store.Snapshot();
        return snapshot.Invoices
            .Where(i => i.ClientId == clientId)
            .Select(i => i.ToResponse())
            .ToList();
    }

    public InvoiceResponse GetById(Guid id)
    {
        var snapshot = store.Snapshot();
        var invoice  = snapshot.Invoices.FirstOrDefault(i => i.Id == id)
                       ?? throw new NotFoundException($"Invoice '{id}' not found.");
        return invoice.ToResponse();
    }

    public InvoiceResponse Create(CreateInvoiceRequest request)
    {
        // Uniqueness check
        var snapshot = store.Snapshot();
        if (snapshot.Invoices.Any(i => i.ClientId == request.ClientId
                                    && i.InvoiceNumber.Equals(
                                           request.InvoiceNumber.Trim(),
                                           StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException(
                $"Invoice number '{request.InvoiceNumber}' already exists for this client.");
        }

        var invoice = request.ToDomain(Guid.NewGuid());

        store.BeginTransaction();
        try
        {
            store.AddInvoice(invoice);
            store.Commit();
        }
        catch
        {
            store.Rollback();
            throw;
        }

        return invoice.ToResponse();
    }

    public void Delete(Guid id)
    {
        var snapshot = store.Snapshot();
        _ = snapshot.Invoices.FirstOrDefault(i => i.Id == id)
            ?? throw new NotFoundException($"Invoice '{id}' not found.");

        store.BeginTransaction();
        try
        {
            store.RemoveInvoice(id);
            store.Commit();
        }
        catch
        {
            store.Rollback();
            throw;
        }
    }
}
```

---

## Controller (`Controllers/InvoicesController.cs`)

```csharp
using CaManagement.Api.DTOs.Invoices;
using CaManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InvoicesController(IInvoiceService svc) : ControllerBase
{
    [HttpGet("by-client/{clientId:guid}")]
    [ProducesResponseType<IReadOnlyList<InvoiceResponse>>(StatusCodes.Status200OK)]
    public IActionResult GetByClient([FromRoute] Guid clientId)
        => Ok(svc.GetAll(clientId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById([FromRoute] Guid id)
        => Ok(svc.GetById(id));

    [HttpPost]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Create([FromBody] CreateInvoiceRequest request)
    {
        var result = svc.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] Guid id)
    {
        svc.Delete(id);
        return NoContent();
    }
}
```

---

## DI registration (one line added to `Configurations/ServiceCollectionExtensions.cs`)

```csharp
services.AddScoped<IInvoiceService, InvoiceService>();
```

---

## Naming quick-reference

| Artifact                      | Pattern                              | Example                   |
|-------------------------------|--------------------------------------|---------------------------|
| Model                         | `Models/{Name}.cs`                   | `Models/Invoice.cs`       |
| Enum                          | `Models/{Name}.cs`                   | `Models/InvoiceStatus.cs` |
| Response DTO                  | `DTOs/{Names}/{Name}Response.cs`     | `DTOs/Invoices/InvoiceResponse.cs` |
| Create request DTO            | `DTOs/{Names}/Create{Name}Request.cs`| `DTOs/Invoices/CreateInvoiceRequest.cs` |
| Update request DTO            | `DTOs/{Names}/Update{Name}Request.cs`| `DTOs/Invoices/UpdateInvoiceRequest.cs` |
| Service interface             | `Services/I{Name}Service.cs`         | `Services/IInvoiceService.cs` |
| Service implementation        | `Services/{Name}Service.cs`          | `Services/InvoiceService.cs` |
| Controller                    | `Controllers/{Name}sController.cs`   | `Controllers/InvoicesController.cs` |
