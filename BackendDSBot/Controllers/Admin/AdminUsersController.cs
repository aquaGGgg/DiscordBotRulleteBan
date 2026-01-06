using Application.UseCases.Admin.Users;
using Contracts.Admin.Users;
using Contracts.Bot.Me;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.Admin;

[ApiController]
[Route("admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ListUsersHandler _handler;

    public AdminUsersController(ListUsersHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<AdminUsersResponse>> Get([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
    {
        var res = await _handler.HandleAsync(new ListUsersQuery(limit, offset), ct);

        var dto = res.Users.Select(u =>
        {
            ActivePunishmentDto? ap = u.ActivePunishment is null
                ? null
                : new ActivePunishmentDto(u.ActivePunishment.Id, u.ActivePunishment.GuildId, u.ActivePunishment.EndsAt, u.ActivePunishment.PriceTickets, u.ActivePunishment.Status);

            return new AdminUserItem(u.UserId, u.DiscordUserId, u.TicketsBalance, u.CreatedAt, u.UpdatedAt, ap);
        }).ToList();

        return Ok(new AdminUsersResponse(dto));
    }
}
