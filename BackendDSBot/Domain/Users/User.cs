using Domain.Shared;

namespace Domain.Users;

public sealed class User
{
    public Guid Id { get; }
    public string DiscordUserId { get; private set; }
    public int TicketsBalance { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User(Guid id, string discordUserId, int ticketsBalance, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Guard.NotNullOrWhiteSpace(discordUserId, nameof(discordUserId));
        Guard.NonNegative(ticketsBalance, nameof(ticketsBalance));

        Id = id;
        DiscordUserId = discordUserId;
        TicketsBalance = ticketsBalance;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public void SetDiscordUserId(string discordUserId, DateTimeOffset now)
    {
        Guard.NotNullOrWhiteSpace(discordUserId, nameof(discordUserId));
        DiscordUserId = discordUserId;
        UpdatedAt = now;
    }

    public void AddTickets(int amount, DateTimeOffset now)
    {
        Guard.Positive(amount, nameof(amount));
        checked { TicketsBalance += amount; }
        UpdatedAt = now;
    }

    public void RemoveTickets(int amount, DateTimeOffset now)
    {
        Guard.Positive(amount, nameof(amount));
        if (TicketsBalance - amount < 0)
            throw new DomainException("Insufficient tickets.");
        TicketsBalance -= amount;
        UpdatedAt = now;
    }

    public void SetTickets(int newBalance, DateTimeOffset now)
    {
        Guard.NonNegative(newBalance, nameof(newBalance));
        TicketsBalance = newBalance;
        UpdatedAt = now;
    }
}
