namespace CaManagement.Api.DTOs.Accounts;

public sealed class AccountResponse
{
    public required Guid Id { get; init; }
    public required Guid ClientId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal ComputedBalance { get; init; }
    public required string Currency { get; init; }
}
