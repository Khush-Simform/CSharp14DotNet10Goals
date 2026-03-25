using CaManagement.Api.Models;

namespace CaManagement.Api.Helpers;

/// <summary>Deterministic seed data for demonstration (no external persistence).</summary>
public static class SeedData
{
    public static CaDataSnapshot CreateInitial(TimeProvider time)
    {
        var now = time.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var clientA = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var clientB = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var accCash = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accFees = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var accGst = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var accBank = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        Client[] clients =
        [
            new Client
            {
                Id = clientA,
                LegalName = "Indus Manufacturing Pvt Ltd",
                TradeName = "Indus Mfg",
                TaxIdentifier = "AABCI1234F",
                Email = "cfo@indusmfg.example",
                Phone = "+91-22-4000-1000",
                EngagementDate = new DateOnly(2022, 4, 1),
                IsActive = true
            },
            new Client
            {
                Id = clientB,
                LegalName = "Harbor Retail LLP",
                TradeName = "Harbor Retail",
                TaxIdentifier = "AABCH9876Z",
                Email = "accounts@harborretail.example",
                Phone = "+91-80-2200-3300",
                EngagementDate = new DateOnly(2023, 1, 15),
                IsActive = true
            }
        ];

        Account[] accounts =
        [
            new Account
            {
                Id = accCash,
                ClientId = clientA,
                Code = "1000",
                Name = "Cash on Hand",
                Category = AccountCategory.Asset,
                OpeningBalance = 250_000m,
                Currency = "INR"
            },
            new Account
            {
                Id = accBank,
                ClientId = clientA,
                Code = "1010",
                Name = "HDFC Current",
                Category = AccountCategory.Asset,
                OpeningBalance = 1_850_000m,
                Currency = "INR"
            },
            new Account
            {
                Id = accFees,
                ClientId = clientA,
                Code = "4000",
                Name = "Professional Fees",
                Category = AccountCategory.Income,
                OpeningBalance = 0m,
                Currency = "INR"
            },
            new Account
            {
                Id = accGst,
                ClientId = clientB,
                Code = "2310",
                Name = "GST Payable",
                Category = AccountCategory.Liability,
                OpeningBalance = 42_500m,
                Currency = "INR"
            }
        ];

        CaTransaction[] transactions =
        [
            new CaTransaction
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000001"),
                AccountId = accFees,
                Amount = 118_000m,
                Direction = TransactionDirection.Credit,
                Narration = "Monthly retainer — statutory compliance",
                Reference = "INV-2025-03",
                PostedAt = now.AddDays(-12),
                VoucherNumber = "JV-2401"
            },
            new CaTransaction
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000002"),
                AccountId = accBank,
                Amount = 65_000m,
                Direction = TransactionDirection.Debit,
                Narration = "Client reimbursement — filing fees",
                Reference = "CHQ-88912",
                PostedAt = now.AddDays(-10),
                VoucherNumber = "PV-1188"
            },
            new CaTransaction
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000003"),
                AccountId = accGst,
                Amount = 18_000m,
                Direction = TransactionDirection.Credit,
                Narration = "GST liability accrual — Q4",
                Reference = "GST-RPT",
                PostedAt = now.AddDays(-5),
                VoucherNumber = "JE-441"
            }
        ];

        AuditReport[] reports =
        [
            new AuditReport
            {
                Id = Guid.Parse("e2000000-0000-0000-0000-000000000001"),
                ClientId = clientA,
                PeriodStart = new DateOnly(today.Year, 1, 1),
                PeriodEnd = today,
                Title = "Limited Review — Q1 Operating effectiveness",
                Findings =
                [
                    "Bank reconciliations completed within SLA.",
                    "Two expense vouchers missing secondary approval (cleared post period).",
                    "Revenue recognition aligned with engagement letters."
                ],
                TotalDebits = 65_000m,
                TotalCredits = 118_000m,
                GeneratedAt = now.AddDays(-1)
            }
        ];

        return new CaDataSnapshot
        {
            Clients = [.. clients],
            Accounts = [.. accounts],
            Transactions = [.. transactions],
            AuditReports = [.. reports]
        };
    }
}
