using CaManagement.Api.DTOs.Audit;

namespace CaManagement.Api.Services;

public interface IAuditService
{
    Task<IReadOnlyList<AuditReportResponse>> ListAsync(Guid? clientId, CancellationToken cancellationToken);
    Task<AuditSummaryResponse> GenerateSummaryAsync(Guid? clientId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
    Task<AuditReportResponse> PersistGeneratedReportAsync(AuditSummaryResponse summary, string title, CancellationToken cancellationToken);
}
