using CaManagement.Api.Models;

namespace CaManagement.Api.Helpers;

/// <summary>Pattern matching over ledger lines for audit narrative hints.</summary>
public static class TransactionAnalytics
{
    public static string DescribeRisk(CaTransaction t) => (t.Amount, t.Direction, t.Narration) switch
    {
        ( > 500_000m, _, _) => "High-value movement — escalate partner review.",
        ( _, TransactionDirection.Debit, var n) when n.Contains("reimbursement", StringComparison.OrdinalIgnoreCase) =>
            "Debit reimbursement — verify supporting vouchers.",
        ( _, TransactionDirection.Credit, var n) when n.Contains("GST", StringComparison.OrdinalIgnoreCase) =>
            "GST-related credit — reconcile returns.",
        ( < 1_000m, _, _) => "Routine low-value entry.",
        _ => "Standard ledger posting."
    };
}
