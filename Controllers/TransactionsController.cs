using CaManagement.Api.DTOs.Transactions;
using CaManagement.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace CaManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController(ITransactionService transactions) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Search ledger postings", Description = "Filter by account, client, or narration substring.")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> SearchAsync(
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? clientId,
        [FromQuery] string? narration,
        CancellationToken cancellationToken)
    {
        var rows = await transactions.SearchAsync(accountId, clientId, narration, cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Record a single ledger posting")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> RecordAsync(
        [FromBody] RecordTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await transactions.RecordAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPost("batch")]
    [SwaggerOperation(
        Summary = "Simulated in-memory batch",
        Description = "Wraps postings in Begin/Commit or Rollback on the in-memory store (no real database transaction).")]
    [ProducesResponseType(typeof(SimulatedBatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SimulatedBatchResponse>> BatchAsync(
        [FromBody] SimulatedBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await transactions.RunSimulatedBatchAsync(request, cancellationToken);
        return Ok(result);
    }
}
