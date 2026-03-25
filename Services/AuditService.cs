using CaManagement.Api.DTOs.Audit;
using CaManagement.Api.Helpers;
using CaManagement.Api.Models;
using CaManagement.Api.Services.Exceptions;

namespace CaManagement.Api.Services;

public sealed class AuditService(CaDataStore store, TimeProvider time) : IAuditService
{
    public Task<IReadOnlyList<AuditReportResponse>> ListAsync(Guid? clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var list = store.Query(snapshot =>
        {
            IEnumerable<AuditReport> q = snapshot.AuditReports;
            if (clientId is { } cid)
                q = q.Where(r => r.ClientId == cid);
            return q.OrderByDescending(r => r.GeneratedAt).Select(r => r.ToResponse()).ToList();
        });
        return Task.FromResult<IReadOnlyList<AuditReportResponse>>(list);
    }

    public Task<AuditSummaryResponse> GenerateSummaryAsync(Guid? clientId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (to < from)
            throw new DomainValidationException("Period end must be on or after period start.");

        var summary = store.Query(snapshot =>
        {
            IEnumerable<CaTransaction> q = snapshot.Transactions;
            if (clientId is { } cid)
            {
                var accountIds = snapshot.Accounts.Where(a => a.ClientId == cid).Select(a => a.Id).ToHashSet();
                q = q.Where(t => accountIds.Contains(t.AccountId));
            }

            var inRange = q.Where(t =>
            {
                var d = DateOnly.FromDateTime(t.PostedAt.UtcDateTime);
                return d >= from && d <= to;
            }).ToList();

            var debitAmounts = inRange.Where(t => t.Direction == TransactionDirection.Debit).Select(t => t.Amount).ToArray();
            var creditAmounts = inRange.Where(t => t.Direction == TransactionDirection.Credit).Select(t => t.Amount).ToArray();
            var debits = FinancialMath.Sum(debitAmounts);
            var credits = FinancialMath.Sum(creditAmounts);
            var topNarrations = inRange
                .GroupBy(t => t.Narration.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Sum(x => x.Amount))
                .Take(5)
                .Select(g => $"{g.Key} ({g.Count()} postings)")
                .ToList();

            return new AuditSummaryResponse
            {
                ClientId = clientId,
                PeriodStart = from,
                PeriodEnd = to,
                TransactionCount = inRange.Count,
                TotalDebits = debits,
                TotalCredits = credits,
                Net = credits - debits,
                TopNarrations = topNarrations
            };
        });

        return Task.FromResult(summary);
    }

    public Task<AuditReportResponse> PersistGeneratedReportAsync(AuditSummaryResponse summary, string title, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Title is required.");

        var report = new AuditReport
        {
            Id = Guid.NewGuid(),
            ClientId = summary.ClientId,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd,
            Title = title.Trim(),
            Findings =
            [
                $"Transactions reviewed: {summary.TransactionCount}.",
                $"Total debits: {summary.TotalDebits:N2}; total credits: {summary.TotalCredits:N2}.",
                .. summary.TopNarrations.Take(3).Select(n => $"Notable: {n}")
            ],
            TotalDebits = summary.TotalDebits,
            TotalCredits = summary.TotalCredits,
            GeneratedAt = time.GetUtcNow()
        };

        store.Mutate(s => s.AuditReports.Add(report));
        return Task.FromResult(report.ToResponse());
    }
}
