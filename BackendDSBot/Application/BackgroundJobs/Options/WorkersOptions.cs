namespace Application.BackgroundJobs.Options;

public sealed class WorkersOptions
{
    public ExpirePunishmentsOptions ExpirePunishments { get; init; } = new();
    public RouletteOptions Roulette { get; init; } = new();

    // Если у тебя пока один guild — можно держать тут
    public string DefaultGuildId { get; init; } = "default";
}

public sealed class ExpirePunishmentsOptions
{
    public bool Enabled { get; init; } = true;
    public int IntervalSeconds { get; init; } = 10;
    public int BatchSize { get; init; } = 100;
}

public sealed class RouletteOptions
{
    public bool Enabled { get; init; } = true;

    // Как часто воркер “тикает”. Он может тикать часто — хендлеры защищены bucket’ом.
    public int TickSeconds { get; init; } = 5;
}
