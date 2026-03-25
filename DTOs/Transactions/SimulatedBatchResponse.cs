namespace CaManagement.Api.DTOs.Transactions;

public sealed class SimulatedBatchResponse
{
    public required bool Committed { get; init; }
    public required IReadOnlyList<TransactionResponse> Transactions { get; init; }
}
