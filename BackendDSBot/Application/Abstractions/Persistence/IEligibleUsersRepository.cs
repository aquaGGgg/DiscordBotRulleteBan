namespace Application.Abstractions.Persistence;

public interface IEligibleUsersRepository
{
    Task<IReadOnlyList<string>> GetEligibleDiscordUserIdsAsync(string guildId, int limit, CancellationToken ct);

    Task UpsertSnapshotAsync(string guildId, IReadOnlyCollection<string> eligibleDiscordUserIds, CancellationToken ct);
}
