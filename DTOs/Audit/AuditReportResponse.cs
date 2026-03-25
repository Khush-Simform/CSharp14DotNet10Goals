namespace CaManagement.Api.DTOs.Audit;

public sealed class AuditReportResponse
{
    public required Guid Id { get; init; }
    public Guid? ClientId { get; init; }
    public required DateOnly PeriodStart { get; init; }
    public required DateOnly PeriodEnd { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<string> Findings { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal NetMovement { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
}
