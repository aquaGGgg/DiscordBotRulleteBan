using Application.UseCases.Admin.Tickets;
using Contracts.Admin.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Admin;

[ApiController]
[Route("admin/tickets")]
public sealed class AdminTicketsController : ControllerBase
{
    private readonly AdjustTicketsHandler _handler;

    public AdminTicketsController(AdjustTicketsHandler handler) => _handler = handler;

    [HttpPost("adjust")]
    public async Task<ActionResult<AdjustTicketsResponse>> Adjust([FromBody] AdjustTicketsRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.DiscordUserId) || req.Delta == 0)
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "DiscordUserId is required and Delta must be non-zero." });

        var res = await _handler.HandleAsync(new AdjustTicketsCommand(req.DiscordUserId, req.Delta), ct);
        return Ok(new AdjustTicketsResponse(res.UserId, res.NewBalance));
    }
}
