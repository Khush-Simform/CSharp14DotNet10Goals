using CaManagement.Api.DTOs.Transactions;
using CaManagement.Api.Helpers;
using CaManagement.Api.Models;
using CaManagement.Api.Services.Exceptions;

namespace CaManagement.Api.Services;

public sealed class TransactionService(CaDataStore store, TimeProvider time) : ITransactionService
{
    public Task<TransactionResponse> RecordAsync(RecordTransactionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureAccountExists(request.AccountId);
        var id = Guid.NewGuid();
        var postedAt = time.GetUtcNow();
        var entity = request.ToDomain(id, postedAt);
        store.Mutate(s => s.Transactions.Add(entity));
        return Task.FromResult(entity.ToResponse());
    }

    public Task<IReadOnlyList<TransactionResponse>> SearchAsync(
        Guid? accountId,
        Guid? clientId,
        string? narrationContains,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hint = narrationContains?.Trim();
        var list = store.Query(snapshot =>
        {
            IEnumerable<CaTransaction> q = snapshot.Transactions;
            if (accountId is { } aid)
                q = q.Where(t => t.AccountId == aid);
            if (clientId is { } cid)
            {
                var accountIds = snapshot.Accounts.Where(a => a.ClientId == cid).Select(a => a.Id).ToHashSet();
                q = q.Where(t => accountIds.Contains(t.AccountId));
            }

            if (!string.IsNullOrEmpty(hint))
                q = q.Where(t => t.Narration.Contains(hint, StringComparison.OrdinalIgnoreCase));

            return q.OrderByDescending(t => t.PostedAt).Select(t => t.ToResponse()).ToList();
        });
        return Task.FromResult<IReadOnlyList<TransactionResponse>>(list);
    }

    public Task<SimulatedBatchResponse> RunSimulatedBatchAsync(SimulatedBatchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Steps.Count == 0)
            throw new DomainValidationException("At least one transaction step is required.");

        foreach (var step in request.Steps)
            EnsureAccountExists(step.AccountId);

        store.BeginTransaction();
        try
        {
            var created = new List<CaTransaction>();
            foreach (var step in request.Steps)
            {
                var id = Guid.NewGuid();
                var entity = step.ToDomain(id, time.GetUtcNow());
                store.Mutate(s => s.Transactions.Add(entity));
                created.Add(entity);
            }

            var committed = request.Commit;
            if (committed)
                store.Commit();
            else
                store.Rollback();

            var responses = created.Select(c => c.ToResponse()).ToList();
            return Task.FromResult(new SimulatedBatchResponse
            {
                Committed = committed,
                Transactions = responses
            });
        }
        catch
        {
            store.Rollback();
            throw;
        }
    }

    private void EnsureAccountExists(Guid accountId)
    {
        var exists = store.Query(s => s.Accounts.Exists(a => a.Id == accountId));
        if (!exists)
            throw new NotFoundException($"Account '{accountId}' was not found.");
    }
}
