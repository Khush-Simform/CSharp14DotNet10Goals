using CaManagement.Api.DTOs.Accounts;

namespace CaManagement.Api.Services;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetForClientAsync(Guid clientId, CancellationToken cancellationToken);
}
