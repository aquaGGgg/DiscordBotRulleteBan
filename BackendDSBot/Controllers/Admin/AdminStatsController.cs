using Application.UseCases.Admin.Stats;
using Contracts.Admin.Stats;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Admin;

[ApiController]
[Route("admin/stats")]
public sealed class AdminStatsController : ControllerBase
{
    private readonly GetStatsHandler _handler;

    public AdminStatsController(GetStatsHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<AdminStatsResponse>> Get(CancellationToken ct)
    {
        var res = await _handler.HandleAsync(new GetStatsQuery(), ct);
        return Ok(new AdminStatsResponse(
            res.Stats.TotalUsers,
            res.Stats.ActivePunishments,
            res.Stats.PendingJobs,
            res.Stats.ProcessingJobs
        ));
    }
}
