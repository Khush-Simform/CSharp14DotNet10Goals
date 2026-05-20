## Summary
<!-- One-line description of what this PR does -->

Closes #<!-- issue number -->

## Type of Change
- [ ] Bug fix (non-breaking)
- [ ] New feature (non-breaking)
- [ ] Breaking change (existing endpoints / contracts modified)
- [ ] Refactor / internal improvement
- [ ] CI / infrastructure

## Changes
<!-- List the key files changed and why -->

| File | Change |
|------|--------|
|  |  |

## API Contract Impact
- [ ] No public endpoint added or modified
- [ ] New endpoint added (Swagger annotation included)
- [ ] Existing endpoint signature changed (version bump considered)
- [ ] DTO shape changed (backwards-compatible)

## Finance Domain Checklist
- [ ] All monetary amounts use `decimal` (no `float` / `double`)
- [ ] Debit/Credit direction is explicit — no implicit sign convention
- [ ] `FinancialMath.Sum` used instead of raw LINQ `.Sum()` for variadic paths
- [ ] `TransactionAnalytics.DescribeRisk` covers the new narration patterns (if applicable)
- [ ] Audit trail preserved — no silent data mutation

## Testing
- [ ] Built and run locally against seed data (Indus Mfg + Harbor Retail)
- [ ] Swagger UI (`/swagger`) exercised for affected endpoints
- [ ] `.http` file updated / added for new endpoints
- [ ] Simulated batch rollback tested where relevant

## Security
- [ ] No secrets, connection strings, or tokens committed
- [ ] Input validated at controller / DTO layer (`[Required]`, `[Range]`, etc.)
- [ ] `ExceptionHandlingMiddleware` hides stack traces in non-Development environments

## Reviewer Notes
<!-- Anything specific you want reviewers to focus on -->
