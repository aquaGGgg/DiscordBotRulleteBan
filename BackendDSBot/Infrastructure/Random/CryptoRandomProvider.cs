using System.Security.Cryptography;
using Application.Abstractions.Random;

namespace Infrastructure.Random;

public sealed class CryptoRandomProvider : IRandomProvider
{
    public int NextInt(int minInclusive, int maxInclusive)
    {
        if (minInclusive > maxInclusive)
            throw new ArgumentOutOfRangeException(nameof(minInclusive), "min > max");

        // RandomNumberGenerator.GetInt32 uses maxExclusive
        if (maxInclusive == int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(maxInclusive), "maxInclusive too large for inclusive generation");

        return RandomNumberGenerator.GetInt32(minInclusive, maxInclusive + 1);
    }

    public IReadOnlyList<int> PickDistinctIndices(int n, int count)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count > n) count = n;

        // Partial Fisher–Yates for first "count" elements
        var arr = new int[n];
        for (var i = 0; i < n; i++) arr[i] = i;

        for (var i = 0; i < count; i++)
        {
            var j = RandomNumberGenerator.GetInt32(i, n);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        var result = new int[count];
        Array.Copy(arr, 0, result, 0, count);
        return result;
    }
}
