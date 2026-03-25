using System.ComponentModel.DataAnnotations;
using CaManagement.Api.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace CaManagement.Api.DTOs.Transactions;

public sealed class RecordTransactionRequest
{
    [Required]
    public required Guid AccountId { get; init; }

    /// <example>25000.00</example>
    [Range(0.01, double.MaxValue)]
    [SwaggerSchema(Description = "Absolute movement amount (direction is explicit).")]
    public required decimal Amount { get; init; }

    [Required]
    public required TransactionDirection Direction { get; init; }

    /// <example>Professional fees — March compliance</example>
    [Required, MinLength(3), MaxLength(512)]
    public required string Narration { get; init; }

    /// <example>INV-2025-09</example>
    [MaxLength(64)]
    public string? Reference { get; init; }

    /// <example>JV-9001</example>
    [MaxLength(32)]
    public string? VoucherNumber { get; init; }
}
