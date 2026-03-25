namespace CaManagement.Api.Helpers;

/// <summary>Demonstrates C# 13+ <c>params</c> collection for variadic numeric helpers.</summary>
public static class FinancialMath
{
    public static decimal Sum(params ReadOnlySpan<decimal> amounts)
    {
        decimal total = 0m;
        foreach (var a in amounts)
            total += a;
        return total;
    }
}
