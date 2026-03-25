using CaManagement.Api.DTOs.Transactions;

namespace CaManagement.Api.Services;

public interface ITransactionService
{
    Task<TransactionResponse> RecordAsync(RecordTransactionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TransactionResponse>> SearchAsync(Guid? accountId, Guid? clientId, string? narrationContains, CancellationToken cancellationToken);
    Task<SimulatedBatchResponse> RunSimulatedBatchAsync(SimulatedBatchRequest request, CancellationToken cancellationToken);
}
