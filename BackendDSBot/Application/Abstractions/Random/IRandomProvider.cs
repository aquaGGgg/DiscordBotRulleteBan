namespace Application.Abstractions.Random;

public interface IRandomProvider
{    
    int NextInt(int minInclusive, int maxInclusive);
    IReadOnlyList<int> PickDistinctIndices(int n, int count);
}
