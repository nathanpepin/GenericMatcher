namespace GenericMatcher.Collections;

/// <summary>
/// Provides a scoped dictionary that automatically returns to the pool when disposed
/// </summary>
public readonly ref struct PooledDictionary<TKey, TValue>(int? capacity = null)
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = DictionaryPool<TKey, TValue>.Get(capacity);

    public Dictionary<TKey, TValue> Dictionary => _dictionary;

    public void Dispose()
    {
        DictionaryPool<TKey, TValue>.Return(_dictionary);
    }
}