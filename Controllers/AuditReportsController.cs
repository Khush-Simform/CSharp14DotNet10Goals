using CaManagement.Api.DTOs.Audit;
using CaManagement.Api.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CaManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuditReportsController(IAuditService audit) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditReportResponse>>> ListAsync(
        [FromQuery] Guid? clientId,
        CancellationToken cancellationToken)
    {
        var rows = await audit.ListAsync(clientId, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(AuditSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuditSummaryResponse>> SummaryAsync(
        [FromQuery] Guid? clientId,
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        var summary = await audit.GenerateSummaryAsync(clientId, from, to, cancellationToken);
        return Ok(summary);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuditReportResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuditReportResponse>> PersistFromSummaryAsync(
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        [FromQuery] Guid? clientId,
        [FromBody] PersistAuditReportRequest body,
        CancellationToken cancellationToken)
    {
        var summary = await audit.GenerateSummaryAsync(clientId, from, to, cancellationToken);
        var report = await audit.PersistGeneratedReportAsync(summary, body.Title, cancellationToken);
        return CreatedAtAction(nameof(ListAsync), new { clientId }, report);
    }
}
