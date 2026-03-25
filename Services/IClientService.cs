using CaManagement.Api.DTOs.Clients;

namespace CaManagement.Api.Services;

public interface IClientService
{
    Task<IReadOnlyList<ClientResponse>> SearchAsync(string? query, bool? activeOnly, CancellationToken cancellationToken);
    Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken);
}
