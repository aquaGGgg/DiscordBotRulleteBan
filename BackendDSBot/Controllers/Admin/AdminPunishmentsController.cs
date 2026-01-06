using Application.UseCases.Admin.Punishments;
using Contracts.Admin.Punishments;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Admin;

[ApiController]
[Route("admin/punishments")]
public sealed class AdminPunishmentsController : ControllerBase
{
    private readonly ManualBanHandler _manualBan;
    private readonly ReleasePunishmentHandler _release;

    public AdminPunishmentsController(ManualBanHandler manualBan, ReleasePunishmentHandler release)
    {
        _manualBan = manualBan;
        _release = release;
    }

    [HttpPost("manual-ban")]
    public async Task<ActionResult<ManualBanResponse>> ManualBan([FromBody] ManualBanRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId) || string.IsNullOrWhiteSpace(req.DiscordUserId) ||
            req.DurationSeconds <= 0 || req.PriceTickets <= 0)
        {
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId, DiscordUserId, DurationSeconds>0, PriceTickets>0 are required." });
        }

        var res = await _manualBan.HandleAsync(new ManualBanCommand(req.GuildId, req.DiscordUserId, req.DurationSeconds, req.PriceTickets), ct);
        return Ok(new ManualBanResponse(res.PunishmentId, res.EndsAt, res.CreatedNew));
    }

    [HttpPost("release")]
    public async Task<ActionResult<ReleasePunishmentResponse>> Release([FromBody] ReleasePunishmentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId) || string.IsNullOrWhiteSpace(req.DiscordUserId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId and DiscordUserId are required." });

        var res = await _release.HandleAsync(new ReleasePunishmentCommand(req.GuildId, req.DiscordUserId), ct);
        return Ok(new ReleasePunishmentResponse(res.Released, res.PunishmentId));
    }
}
