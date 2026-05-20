using CaManagement.Api.DTOs.Clients;
using CaManagement.Api.Helpers;
using CaManagement.Api.Models;
using CaManagement.Api.Services.Exceptions;

namespace CaManagement.Api.Services;

public sealed class ClientService(CaDataStore store, TimeProvider time) : IClientService
{
    public Task<IReadOnlyList<ClientResponse>> SearchAsync(string? query, bool? activeOnly, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var q = query?.Trim();
        var list = store.Query(snapshot =>
        {
            IEnumerable<Client> rows = snapshot.Clients;
            if (activeOnly is true)
                rows = rows.Where(c => c.IsActive);
            if (!string.IsNullOrEmpty(q))
            {
                rows = rows.Where(c =>
                    c.LegalName.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (c.TradeName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                    || c.TaxIdentifier.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || c.Email.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            return rows.Select(c => c.ToResponse()).ToList();
        });
        return Task.FromResult<IReadOnlyList<ClientResponse>>(list);
    }

    public Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var client = store.Query(s => s.Clients.FirstOrDefault(c => c.Id == id))
            ?? throw new NotFoundException($"Client '{id}' was not found.");
        return Task.FromResult(client.ToResponse());
    }

    public Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var id = Guid.NewGuid();
        var engagement = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var domain = request.ToDomain(id, engagement);
        store.Mutate(s => s.Clients.Add(domain));
        return Task.FromResult(domain.ToResponse());
    }

    public Task<ClientResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var client = store.Query(s => s.Clients.FirstOrDefault(c => c.Id == id))
            ?? throw new NotFoundException($"Client '{id}' was not found.");

        if (!client.IsActive)
            throw new ConflictException($"Client '{id}' is already inactive.");

        store.Mutate(s =>
        {
            var target = s.Clients.First(c => c.Id == id);
            target.IsActive = false;
        });

        var updated = store.Query(s => s.Clients.First(c => c.Id == id));
        return Task.FromResult(updated.ToResponse());
    }
}
