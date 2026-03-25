namespace CaManagement.Api.Models;

/// <summary>Ledger movement for a client account (in-memory PoC).</summary>
public sealed class CaTransaction
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required TransactionDirection Direction { get; init; }
    public required string Narration { get; init; }
    public string? Reference { get; init; }
    public required DateTimeOffset PostedAt { get; init; }
    public string? VoucherNumber { get; init; }
}
