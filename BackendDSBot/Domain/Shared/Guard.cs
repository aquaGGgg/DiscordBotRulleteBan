namespace Domain.Shared;

public static class Guard
{
    public static void NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} is required.");
    }

    public static void NonNegative(int value, string name)
    {
        if (value < 0)
            throw new DomainException($"{name} must be >= 0.");
    }

    public static void Positive(int value, string name)
    {
        if (value <= 0)
            throw new DomainException($"{name} must be > 0.");
    }
}
