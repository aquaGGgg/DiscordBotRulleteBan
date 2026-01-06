using Application.UseCases.Admin.Config;
using Contracts.Admin.Config;
using Microsoft.AspNetCore.Mvc;
using DomainConfig = Domain.Rounds.Config;

namespace Controllers.Admin;

[ApiController]
[Route("admin/config")]
public sealed class AdminConfigController : ControllerBase
{
    private readonly GetConfigHandler _get;
    private readonly UpdateConfigHandler _update;

    public AdminConfigController(GetConfigHandler get, UpdateConfigHandler update)
    {
        _get = get;
        _update = update;
    }

    [HttpGet]
    public async Task<ActionResult<ConfigDto>> Get(CancellationToken ct)
    {
        var res = await _get.HandleAsync(new GetConfigQuery(), ct);
        return Ok(ToDto(res.Config));
    }

    [HttpPut]
    public async Task<ActionResult<ConfigDto>> Put([FromBody] UpdateConfigRequest req, CancellationToken ct)
    {
        var res = await _update.HandleAsync(new UpdateConfigCommand(
            req.BanRouletteIntervalSeconds,
            req.BanRoulettePickCount,
            req.BanRouletteDurationMinSeconds,
            req.BanRouletteDurationMaxSeconds,
            req.TicketRouletteIntervalSeconds,
            req.TicketRoulettePickCount,
            req.TicketRouletteTicketsMin,
            req.TicketRouletteTicketsMax,
            req.EligibleRoleId,
            req.JailVoiceChannelId
        ), ct);

        return Ok(ToDto(res.Config));
    }

    private static ConfigDto ToDto(DomainConfig cfg) =>
        new(
            cfg.BanRouletteIntervalSeconds,
            cfg.BanRoulettePickCount,
            cfg.BanRouletteDurationMinSeconds,
            cfg.BanRouletteDurationMaxSeconds,
            cfg.TicketRouletteIntervalSeconds,
            cfg.TicketRoulettePickCount,
            cfg.TicketRouletteTicketsMin,
            cfg.TicketRouletteTicketsMax,
            cfg.EligibleRoleId,
            cfg.JailVoiceChannelId,
            cfg.UpdatedAt
        );
}
