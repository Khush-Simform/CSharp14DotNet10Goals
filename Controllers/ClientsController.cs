using CaManagement.Api.DTOs.Clients;
using CaManagement.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace CaManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClientsController(IClientService clients) : ControllerBase
{
    /// <summary>Full-text style search across name, trade name, tax id, and email.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Search clients", Description = "Optional filters: query substring and active-only flag.")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> SearchAsync(
        [FromQuery] string? q,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await clients.SearchAsync(q, activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await clients.GetByIdAsync(id, cancellationToken);
        return Ok(client);
    }

    /// <summary>Registers a new client engagement record.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientResponse>> CreateAsync(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var created = await clients.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }
}
