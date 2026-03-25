namespace CaManagement.Api.Models;

public sealed class Account
{
    public required Guid Id { get; init; }
    public required Guid ClientId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required AccountCategory Category { get; init; }
    public decimal OpeningBalance { get; init; }
    public string Currency { get; init; } = "INR";
}
