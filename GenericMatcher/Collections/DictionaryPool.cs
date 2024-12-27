using System.Runtime.CompilerServices;

namespace GenericMatcher.Collections;

/// <summary>
///     Provides a pooled dictionary implementation for high-performance scenarios
/// </summary>
public sealed class DictionaryPool<TKey, TValue> where TKey : notnull
{
    private static readonly ObjectPool<Dictionary<TKey, TValue>> Pool;

    // Initialize the pool with a default policy
    static DictionaryPool()
    {
        Pool = new ObjectPool<Dictionary<TKey, TValue>>();
    }

    /// <summary>
    ///     Gets a dictionary from the pool with an optional initial capacity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<TKey, TValue> Get(int? capacity = null)
    {
        var dictionary = Pool.Get();
        dictionary.Clear();

        if (!capacity.HasValue || dictionary.EnsureCapacity(capacity.Value) >= capacity.Value) return dictionary;

        // If the existing dictionary is too small, create a new one with the desired capacity
        Pool.Return(dictionary);
        return new Dictionary<TKey, TValue>(capacity.Value);
    }

    /// <summary>
    ///     Returns a dictionary to the pool
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null!) return;

        dictionary.Clear();
        Pool.Return(dictionary);
    }
}