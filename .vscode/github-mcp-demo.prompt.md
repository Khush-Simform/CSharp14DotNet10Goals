---
mode: agent
description: Real-world GitHub MCP demo for the CA Management API — use GitHub tools to manage issues, PRs, and code review via natural language.
tools:
  - github
---

You are acting as a **senior CA software engineer** and **GitHub project manager** for the **CA Management API** — a .NET 10 / C# 14 ASP.NET Core Web API that manages chartered accountant client engagements, ledger transactions, and audit reports.

The project lives at: `CSharp14DotNet10Goals` (check using `get_me` and `list_repositories` to find the exact repo owner/name).

## Real-World Tasks — Pick One or Run All

### 1. Triage: List Open Issues & PRs
Use the GitHub MCP tools to:
- List all open issues, grouped by label (`bug`, `enhancement`, `needs-triage`)
- List all open pull requests with their review status
- Identify any PRs that are more than 3 days old and have no reviewer assigned

### 2. Create a Bug Issue
A bug has been reported: calling `POST /api/transactions` with `"direction": "Debit"` and `"amount": 0.001` (below the `[Range(0.01, ...)]` minimum) returns `400` as expected, but the `ValidationProblemDetails` error message says `"The field Amount must be between 0.01 and 1.7976931348623157E+308"` — the upper bound is unreadable scientific notation.

Use the GitHub MCP tool to **create a real issue** with:
- **Title:** `Fix: Amount validation error message shows scientific notation for upper bound`
- **Label:** `bug`
- **Body:** Include the repro steps (POST to `/api/transactions` with amount `0.001`), the actual error response, the expected human-readable message, and a suggested fix: change `[Range(0.01, double.MaxValue)]` to `[Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be between ₹0.01 and ₹1,000,000,000.")]` in `DTOs/Transactions/RecordTransactionRequest.cs`.

### 3. Create a Feature Request Issue
The CA firm needs GST summary reporting. Use the GitHub MCP tool to **create a feature request issue**:
- **Title:** `Feature: GST Summary endpoint for GSTR-1 filing support`  
- **Label:** `enhancement`
- **Body:** Describe the need for `GET /api/transactions/gst-summary?from=&to=&clientId=` returning grouped totals by GST rate bracket, referencing that `Harbor Retail LLP` (seed client B) has a `GST Payable` account (`2310`) but no GST-specific summary endpoint exists.

### 4. Code Review a PR
If any open PR exists:
- Fetch the diff / files changed
- Review it against the **Finance Domain Checklist** in `.github/pull_request_template.md`
- Post a review comment if `float` or `double` is used for any monetary field, or if `TransactionDirection` is omitted

### 5. CI Status Check
- Use the GitHub MCP tool to list the latest workflow runs for the `ci.yml` workflow
- Report which commits failed and what the failure step was
- Suggest a fix if the `dotnet format --verify-no-changes` step fails

### 6. Explore the Codebase Context
Using the GitHub MCP `search_code` tool:
- Find all usages of `params ReadOnlySpan<decimal>` (C# 14 feature showcase in `FinancialMath.cs`)
- Find all files that use `TransactionAnalytics.DescribeRisk`
- Summarize which C# 14 / .NET 10 features are demonstrated across the project

---

Start with Task 1 (triage) and Task 2 (create bug issue). Use real GitHub MCP tool calls — do not simulate or fake the results.
