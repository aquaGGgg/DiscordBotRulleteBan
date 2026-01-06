using Application.UseCases.Bot.Tickets;
using Contracts.Bot.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot/tickets")]
public sealed class BotTicketsController : ControllerBase
{
    private readonly TransferTicketsHandler _transfer;

    public BotTicketsController(TransferTicketsHandler transfer) => _transfer = transfer;

    [HttpPost("transfer")]
    public async Task<ActionResult<TransferTicketsResponse>> Transfer([FromBody] TransferTicketsRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FromDiscordUserId) ||
            string.IsNullOrWhiteSpace(req.ToDiscordUserId) ||
            req.Amount <= 0)
        {
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "FromDiscordUserId, ToDiscordUserId and Amount>0 are required." });
        }

        var res = await _transfer.HandleAsync(new TransferTicketsCommand(req.FromDiscordUserId, req.ToDiscordUserId, req.Amount), ct);
        return Ok(new TransferTicketsResponse(res.FromUserId, res.ToUserId, res.Amount));
    }
}
