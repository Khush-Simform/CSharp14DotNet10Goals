namespace CaManagement.Api.DTOs.Clients;

public sealed class ClientResponse
{
    public required Guid Id { get; init; }
    public required string LegalName { get; init; }
    public string? TradeName { get; init; }
    public required string TaxIdentifier { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }
    public required DateOnly EngagementDate { get; init; }
    public bool IsActive { get; init; }
}
