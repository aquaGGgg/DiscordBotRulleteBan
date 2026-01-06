using Application.Abstractions.Random;

namespace Infrastructure.Random;

public sealed class SystemRandomProvider : IRandomProvider
{
    private readonly System.Random _rng = new();

    // inclusive min, inclusive max
    public int NextInt(int minInclusive, int maxInclusive)
    {
        if (minInclusive > maxInclusive)
            (minInclusive, maxInclusive) = (maxInclusive, minInclusive);

        return _rng.Next(minInclusive, maxInclusive + 1); // max is exclusive
    }

    public IReadOnlyList<int> PickDistinctIndices(int count, int take)
    {
        if (count <= 0 || take <= 0) return Array.Empty<int>();
        take = Math.Min(take, count);

        // Fisher–Yates partial shuffle
        var arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;

        for (int i = 0; i < take; i++)
        {
            var j = _rng.Next(i, count);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        var res = new int[take];
        Array.Copy(arr, res, take);
        return res;
    }
}
