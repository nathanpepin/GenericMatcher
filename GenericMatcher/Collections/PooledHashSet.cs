namespace GenericMatcher.Collections;

public readonly ref struct PooledHashSet<T>(int? capacity = null, IEqualityComparer<T>? comparer = null)
{
    private readonly HashSet<T> _hashSet = HashSetPool<T>.Get(capacity, comparer);

    public HashSet<T> HashSet => _hashSet;

    public void Dispose()
    {
        HashSetPool<T>.Return(_hashSet);
    }
}