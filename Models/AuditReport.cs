namespace CaManagement.Api.Models;

public sealed class AuditReport
{
    public required Guid Id { get; init; }
    public Guid? ClientId { get; init; }
    public required DateOnly PeriodStart { get; init; }
    public required DateOnly PeriodEnd { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<string> Findings { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
}
