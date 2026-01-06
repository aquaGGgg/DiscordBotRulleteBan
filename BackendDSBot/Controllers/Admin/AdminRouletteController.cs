using Application.UseCases.Admin.Roulette;
using Contracts.Admin.Roulette;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Admin;

[ApiController]
[Route("admin/roulette")]
public sealed class AdminRouletteController : ControllerBase
{
    private readonly RunBanRouletteHandler _ban;
    private readonly RunTicketRouletteHandler _ticket;

    public AdminRouletteController(RunBanRouletteHandler ban, RunTicketRouletteHandler ticket)
    {
        _ban = ban;
        _ticket = ticket;
    }

    [HttpPost("ban/run")]
    public async Task<ActionResult<RunBanRouletteResponse>> RunBan([FromBody] RunBanRouletteRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId is required." });

        var createdBy = string.IsNullOrWhiteSpace(req.CreatedBy) ? "Admin" : req.CreatedBy!;
        var res = await _ban.HandleAsync(new RunBanRouletteCommand(req.GuildId, createdBy), ct);

        return Ok(new RunBanRouletteResponse(res.Ran, res.Bucket, res.PickedCount));
    }

    [HttpPost("ticket/run")]
    public async Task<ActionResult<RunTicketRouletteResponse>> RunTicket([FromBody] RunTicketRouletteRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.GuildId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "GuildId is required." });

        var createdBy = string.IsNullOrWhiteSpace(req.CreatedBy) ? "Admin" : req.CreatedBy!;
        var res = await _ticket.HandleAsync(new RunTicketRouletteCommand(req.GuildId, createdBy), ct);

        return Ok(new RunTicketRouletteResponse(res.Ran, res.Bucket, res.PickedCount));
    }
}
