using Application.UseCases.Bot.Eligible;
using Contracts.Bot.Eligible;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot/eligible-users")]
public sealed class BotEligibleUsersController : ControllerBase
{
    private readonly SyncEligibleUsersHandler _handler;

    public BotEligibleUsersController(SyncEligibleUsersHandler handler) => _handler = handler;

    [HttpPost("sync")]
    public async Task<ActionResult<SyncEligibleUsersResponse>> Sync([FromBody] SyncEligibleUsersRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId is required." });

        req = req with { DiscordUserIds = req.DiscordUserIds ?? Array.Empty<string>() };

        var res = await _handler.HandleAsync(new SyncEligibleUsersCommand(req.GuildId, req.DiscordUserIds), ct);
        return Ok(new SyncEligibleUsersResponse(res.Count));
    }
}
