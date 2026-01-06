using Application.UseCases.Bot.Users;
using Contracts.Bot.Users;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Bot;

[ApiController]
[Route("bot/users")]
public sealed class BotUsersController : ControllerBase
{
    private readonly UpsertUserHandler _handler;

    public BotUsersController(UpsertUserHandler handler) => _handler = handler;

    [HttpPost("upsert")]
    public async Task<ActionResult<UpsertUserResponse>> Upsert([FromBody] UpsertUserRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.DiscordUserId))
            return BadRequest(new ProblemDetails { Title = "validation_error", Detail = "DiscordUserId is required." });

        var res = await _handler.HandleAsync(new UpsertUserCommand(req.DiscordUserId), ct);
        return Ok(new UpsertUserResponse(res.UserId, res.DiscordUserId));
    }
}
