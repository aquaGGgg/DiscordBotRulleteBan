using Application.Abstractions.Persistence;

namespace Application.UseCases.Bot.Me;

public sealed record GetMeQuery(string GuildId, string DiscordUserId);
public sealed record GetMeResult(BotMeReadModel Me);
