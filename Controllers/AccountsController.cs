using CaManagement.Api.DTOs.Accounts;
using CaManagement.Api.Services;

namespace CaManagement.Api.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/accounts")]
public sealed class AccountsController(IAccountService accounts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> ListForClientAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var rows = await accounts.GetForClientAsync(clientId, cancellationToken);
        return Ok(rows);
    }
}
