using CaManagement.Api.DTOs.Audit;
using CaManagement.Api.DTOs.Clients;
using CaManagement.Api.Services;

namespace CaManagement.Api.Configurations;

public static class CaMinimalEndpoints
{
    public static WebApplication MapCaMinimalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/minimal/v1")
            .WithTags("Minimal API (v1)");

        group.MapGet("/clients", async Task<IResult> (
                string? q,
                bool? activeOnly,
                IClientService clients,
                CancellationToken cancellationToken) =>
            {
                var list = await clients.SearchAsync(q, activeOnly, cancellationToken);
                return TypedResults.Ok(list);
            })
            .WithName("MinimalSearchClients")
            .WithSummary("Search clients (minimal pipeline)");

        group.MapPost("/clients", async Task<IResult> (
                CreateClientRequest body,
                IClientService clients,
                CancellationToken cancellationToken) =>
            {
                var created = await clients.CreateAsync(body, cancellationToken);
                return TypedResults.Created($"/api/clients/{created.Id}", created);
            })
            .AddEndpointFilter<ValidationFilter<CreateClientRequest>>()
            .WithName("MinimalCreateClient");

        group.MapGet("/transactions", async Task<IResult> (
                Guid? accountId,
                Guid? clientId,
                string? narration,
                ITransactionService transactions,
                CancellationToken cancellationToken) =>
            {
                var list = await transactions.SearchAsync(accountId, clientId, narration, cancellationToken);
                return TypedResults.Ok(list);
            })
            .WithName("MinimalSearchTransactions");

        group.MapGet("/audit/summary", async Task<IResult> (
                Guid? clientId,
                DateOnly from,
                DateOnly to,
                IAuditService audit,
                CancellationToken cancellationToken) =>
            {
                var summary = await audit.GenerateSummaryAsync(clientId, from, to, cancellationToken);
                return TypedResults.Ok(summary);
            })
            .WithName("MinimalAuditSummary");

        group.MapPost("/audit/reports", async Task<IResult> (
                Guid? clientId,
                DateOnly from,
                DateOnly to,
                PersistAuditReportRequest body,
                IAuditService audit,
                CancellationToken cancellationToken) =>
            {
                var summary = await audit.GenerateSummaryAsync(clientId, from, to, cancellationToken);
                var report = await audit.PersistGeneratedReportAsync(summary, body.Title, cancellationToken);
                return TypedResults.Created($"/api/AuditReports?clientId={clientId}", report);
            })
            .AddEndpointFilter<ValidationFilter<PersistAuditReportRequest>>()
            .WithName("MinimalPersistAudit");

        return app;
    }
}
