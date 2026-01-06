using Application.UseCases.Bot.Jobs;
using Contracts.Bot.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot/jobs")]
public sealed class BotJobsController : ControllerBase
{
    private readonly PollJobsHandler _poll;
    private readonly MarkJobDoneHandler _done;
    private readonly MarkJobFailedHandler _failed;

    // 60s lock timeout по умолчанию (можно вынести в appsettings)
    private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(60);

    public BotJobsController(PollJobsHandler poll, MarkJobDoneHandler done, MarkJobFailedHandler failed)
    {
        _poll = poll;
        _done = done;
        _failed = failed;
    }

    [HttpGet("poll")]
    public async Task<ActionResult<PollJobsResponse>> Poll([FromQuery] string workerId, [FromQuery] int limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(workerId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "workerId is required." });

        var res = await _poll.HandleAsync(new PollJobsQuery(workerId, limit), DefaultLockTimeout, ct);

        var dto = res.Jobs.Select(j => new BotJobDto(j.Id, j.Type, j.GuildId, j.DiscordUserId, j.PayloadJson, j.Attempts)).ToList();
        return Ok(new PollJobsResponse(dto));
    }

    [HttpPost("{id:guid}/done")]
    public async Task<ActionResult<MarkJobResult>> Done([FromRoute] Guid id, CancellationToken ct)
    {
        var res = await _done.HandleAsync(new MarkJobDoneCommand(id), ct);
        return Ok(new MarkJobResult(res.Ok));
    }

    [HttpPost("{id:guid}/failed")]
    public async Task<ActionResult<MarkJobResult>> Failed([FromRoute] Guid id, [FromBody] MarkFailedRequest req, CancellationToken ct)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Error))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "Error is required." });

        var res = await _failed.HandleAsync(new MarkJobFailedCommand(id, req.Error), ct);
        return Ok(new MarkJobResult(res.Ok));
    }
}
