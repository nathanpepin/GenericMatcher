namespace GenericMatcher.Collections;

/// <summary>
///     Provides a scoped dictionary that automatically returns to the pool when disposed
/// </summary>
public readonly ref struct PooledDictionary<TKey, TValue>(int? capacity = null)
    where TKey : notnull
{
    public Dictionary<TKey, TValue> Dictionary { get; } = DictionaryPool<TKey, TValue>.Get(capacity);

    public void Dispose()
    {
        DictionaryPool<TKey, TValue>.Return(Dictionary);
    }
}