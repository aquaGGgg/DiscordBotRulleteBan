using Application.UseCases.Bot.Punishments;
using Contracts.Bot.Punishments;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot")]
public sealed class BotPunishmentsController : ControllerBase
{
    private readonly SelfUnbanHandler _selfUnban;

    public BotPunishmentsController(SelfUnbanHandler selfUnban) => _selfUnban = selfUnban;

    [HttpPost("self-unban")]
    public async Task<ActionResult<SelfUnbanResponse>> SelfUnban([FromBody] SelfUnbanRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId) || string.IsNullOrWhiteSpace(req.DiscordUserId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId and DiscordUserId are required." });

        var res = await _selfUnban.HandleAsync(new SelfUnbanCommand(req.GuildId, req.DiscordUserId), ct);
        return Ok(new SelfUnbanResponse(res.Released, res.PunishmentId, res.ChargedTickets));
    }
}
