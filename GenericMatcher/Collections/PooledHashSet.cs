namespace GenericMatcher.Collections;

public readonly ref struct PooledHashSet<T>(int? capacity = null, IEqualityComparer<T>? comparer = null)
{
    public HashSet<T> HashSet { get; } = HashSetPool<T>.Get(capacity, comparer);

    public void Dispose()
    {
        HashSetPool<T>.Return(HashSet);
    }
}