namespace Application.Abstractions.Time;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
