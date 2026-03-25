using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace CaManagement.Api.DTOs.Clients;

public sealed class CreateClientRequest
{
    /// <example>Northwind Traders Pvt Ltd</example>
    [Required, MaxLength(200)]
    [SwaggerSchema(Description = "Registered legal name of the client entity.")]
    public required string LegalName { get; init; }

    /// <example>Northwind</example>
    [MaxLength(120)]
    public string? TradeName { get; init; }

    /// <example>AABCN1234C</example>
    [Required, RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "Tax identifier must look like a PAN (AAAAA9999A).")]
    public required string TaxIdentifier { get; init; }

    /// <example>cfo@northwind.example</example>
    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }

    /// <example>+91-11-4000-0000</example>
    [Phone, MaxLength(32)]
    public string? Phone { get; init; }
}
