using System.ComponentModel.DataAnnotations;

namespace CaManagement.Api.DTOs.Transactions;

/// <summary>Simulates a two-step in-memory posting with optional rollback.</summary>
public sealed class SimulatedBatchRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyList<RecordTransactionRequest> Steps { get; init; }

    /// <summary>When false, the in-memory transaction is rolled back after validation.</summary>
    public bool Commit { get; init; } = true;
}
