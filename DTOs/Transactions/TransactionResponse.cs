namespace CaManagement.Api.DTOs.Transactions;

public sealed class TransactionResponse
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public required string Direction { get; init; }
    public required string Narration { get; init; }
    public string? Reference { get; init; }
    public DateTimeOffset PostedAt { get; init; }
    public string? VoucherNumber { get; init; }
    public required string RiskHint { get; init; }
}
