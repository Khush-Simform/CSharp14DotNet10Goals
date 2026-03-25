namespace CaManagement.Api.Models;

/// <summary>High-level chart-of-accounts classification for CA practice ledgers.</summary>
public enum AccountCategory
{
    Asset = 0,
    Liability = 1,
    Income = 2,
    Expense = 3,
    Equity = 4
}
