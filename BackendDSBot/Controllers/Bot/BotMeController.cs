using Application.UseCases.Bot.Me;
using Contracts.Bot.Me;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot")]
public sealed class BotMeController : ControllerBase
{
    private readonly GetMeHandler _handler;

    public BotMeController(GetMeHandler handler) => _handler = handler;

    // Spec: GET /bot/me?discordUserId=...
    // Для мульти-guild: добавляем guildId query (optional). Если не передан — "default".
    [HttpGet("me")]
    public async Task<ActionResult<BotMeResponse>> Me([FromQuery] string discordUserId, [FromQuery] string? guildId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(discordUserId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "discordUserId is required." });

        var g = string.IsNullOrWhiteSpace(guildId) ? "default" : guildId!;

        var res = await _handler.HandleAsync(new GetMeQuery(g, discordUserId), ct);

        var ap = res.Me.ActivePunishment is null
            ? null
            : new ActivePunishmentDto(
                res.Me.ActivePunishment.Id,
                res.Me.ActivePunishment.GuildId,
                res.Me.ActivePunishment.EndsAt,
                res.Me.ActivePunishment.PriceTickets,
                res.Me.ActivePunishment.Status);

        return Ok(new BotMeResponse(res.Me.UserId, res.Me.DiscordUserId, res.Me.TicketsBalance, ap));
    }
}
