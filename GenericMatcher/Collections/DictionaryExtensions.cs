using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections;

public static class DictionaryExtensions
{
    public static Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> ToNullMatchingResults<TEntity, TMatchType>(this IEnumerable<TEntity> it)
        where TEntity : class
        where TMatchType : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(it);

        TEntity[] items = [..it];

        var dictionary = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(items.Length, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var item in items)
        {
            dictionary.Add(item, new MatchingResult<TEntity, TMatchType>());
        }

        return dictionary;
    }


    public static Dictionary<T, T?> ToNullDictionary<T>(
        this IEnumerable<T> it)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(it);

        T[] items = [..it];

        var dictionary = new Dictionary<T, T?>(items.Length, ReferenceEqualityComparer<T>.Instance);

        foreach (var item in items)
        {
            dictionary.Add(item, null);
        }

        return dictionary;
    }
}