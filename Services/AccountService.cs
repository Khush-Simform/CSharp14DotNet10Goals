using CaManagement.Api.DTOs.Accounts;
using CaManagement.Api.Helpers;
using CaManagement.Api.Models;

namespace CaManagement.Api.Services;

public sealed class AccountService(CaDataStore store) : IAccountService
{
    public Task<IReadOnlyList<AccountResponse>> GetForClientAsync(Guid clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = store.Query(snapshot =>
        {
            var accounts = snapshot.Accounts.Where(a => a.ClientId == clientId).ToList();
            return accounts.Select(a => a.ToResponse(ComputeBalance(snapshot, a.Id))).ToList();
        });
        return Task.FromResult<IReadOnlyList<AccountResponse>>(result);
    }

    private static decimal ComputeBalance(Helpers.CaDataSnapshot snapshot, Guid accountId)
    {
        var opening = snapshot.Accounts.FirstOrDefault(a => a.Id == accountId)?.OpeningBalance ?? 0m;
        decimal delta = 0m;
        foreach (var t in snapshot.Transactions.Where(t => t.AccountId == accountId))
        {
            delta += t.Direction == TransactionDirection.Debit ? t.Amount : -t.Amount;
        }

        return opening + delta;
    }
}
