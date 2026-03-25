using CaManagement.Api.DTOs.Accounts;
using CaManagement.Api.DTOs.Audit;
using CaManagement.Api.DTOs.Clients;
using CaManagement.Api.DTOs.Transactions;
using CaManagement.Api.Models;

namespace CaManagement.Api.Helpers;

public static class CaDtoMapper
{
    public static ClientResponse ToResponse(this Client c) => new()
    {
        Id = c.Id,
        LegalName = c.LegalName,
        TradeName = c.TradeName,
        TaxIdentifier = c.TaxIdentifier,
        Email = c.Email,
        Phone = c.Phone,
        EngagementDate = c.EngagementDate,
        IsActive = c.IsActive
    };

    public static Client ToDomain(this CreateClientRequest dto, Guid id, DateOnly engagementDate) => new()
    {
        Id = id,
        LegalName = dto.LegalName.Trim(),
        TradeName = string.IsNullOrWhiteSpace(dto.TradeName) ? null : dto.TradeName.Trim(),
        TaxIdentifier = dto.TaxIdentifier.Trim().ToUpperInvariant(),
        Email = dto.Email.Trim(),
        Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
        EngagementDate = engagementDate,
        IsActive = true
    };

    public static AccountResponse ToResponse(this Account a, decimal computedBalance) => new()
    {
        Id = a.Id,
        ClientId = a.ClientId,
        Code = a.Code,
        Name = a.Name,
        Category = a.Category.ToString(),
        OpeningBalance = a.OpeningBalance,
        ComputedBalance = computedBalance,
        Currency = a.Currency
    };

    public static TransactionResponse ToResponse(this CaTransaction t) => new()
    {
        Id = t.Id,
        AccountId = t.AccountId,
        Amount = t.Amount,
        Direction = t.Direction.ToString(),
        Narration = t.Narration,
        Reference = t.Reference,
        PostedAt = t.PostedAt,
        VoucherNumber = t.VoucherNumber,
        RiskHint = TransactionAnalytics.DescribeRisk(t)
    };

    public static CaTransaction ToDomain(this RecordTransactionRequest dto, Guid id, DateTimeOffset postedAt) => new()
    {
        Id = id,
        AccountId = dto.AccountId,
        Amount = dto.Amount,
        Direction = dto.Direction,
        Narration = dto.Narration.Trim(),
        Reference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
        PostedAt = postedAt,
        VoucherNumber = string.IsNullOrWhiteSpace(dto.VoucherNumber) ? null : dto.VoucherNumber.Trim()
    };

    public static AuditReportResponse ToResponse(this AuditReport r) => new()
    {
        Id = r.Id,
        ClientId = r.ClientId,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        Title = r.Title,
        Findings = r.Findings,
        TotalDebits = r.TotalDebits,
        TotalCredits = r.TotalCredits,
        NetMovement = r.TotalCredits - r.TotalDebits,
        GeneratedAt = r.GeneratedAt
    };
}
