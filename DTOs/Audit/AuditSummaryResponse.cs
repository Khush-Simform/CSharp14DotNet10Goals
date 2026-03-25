namespace CaManagement.Api.DTOs.Audit;

public sealed class AuditSummaryResponse
{
    public Guid? ClientId { get; init; }
    public required DateOnly PeriodStart { get; init; }
    public required DateOnly PeriodEnd { get; init; }
    public int TransactionCount { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal Net { get; init; }
    public required IReadOnlyList<string> TopNarrations { get; init; }
}
