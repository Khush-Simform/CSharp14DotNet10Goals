using CaManagement.Api.Models;

namespace CaManagement.Api.Helpers;

/// <summary>Point-in-time view of in-memory CA data; supports transactional copy/restore.</summary>
public sealed class CaDataSnapshot
{
    public List<Client> Clients { get; init; } = [];
    public List<Account> Accounts { get; init; } = [];
    public List<CaTransaction> Transactions { get; init; } = [];
    public List<AuditReport> AuditReports { get; init; } = [];

    public CaDataSnapshot Clone() => new()
    {
        Clients = [.. Clients],
        Accounts = [.. Accounts],
        Transactions = [.. Transactions],
        AuditReports = [.. AuditReports]
    };
}
