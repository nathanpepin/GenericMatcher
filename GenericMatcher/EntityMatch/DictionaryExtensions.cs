using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace GenericMatcher.EntityMatch;

public static class DictionaryExtensions
{
    public static Dictionary<T, T?> ToNullDictionary<T>(
        this IEnumerable<T> it)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(it);

        T[] items = [..it];

        var dictionary = new Dictionary<T, T?>(items.Length, ReferenceEqualityComparer<T>.Instance);

        InitializeDictionary(items, dictionary);
        return dictionary;
    }

    public static ConcurrentDictionary<T, T?> ToConcurrentNullDictionary<T>(
        this IEnumerable<T> it)
        where T : class
    {
        T[] items = [..it];

        var dictionary = new ConcurrentDictionary<T, T?>(
            Environment.ProcessorCount,
            items.Length,
            ReferenceEqualityComparer<T>.Instance);

        foreach (var item in items)
        {
            dictionary.TryAdd(item, null);
        }

        return dictionary;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitializeDictionary<T>(
        T[] items,
        Dictionary<T, T?> dictionary)
        where T : class
    {
        foreach (var item in items)
        {
            dictionary.Add(item, null);
        }
    }

    public static Dictionary<T, T?> ToNullDictionaryParallel<T>(
        this IEnumerable<T> it,
        int threshold = 10_000)
        where T : class
    {
        var items = it.ToArray();

        if (items.Length < threshold)
        {
            return items.ToNullDictionary();
        }

        var dictionary = new ConcurrentDictionary<T, T?>(
            Environment.ProcessorCount,
            items.Length,
            ReferenceEqualityComparer<T>.Instance);

        Parallel.ForEach(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            item => dictionary.TryAdd(item, null));

        return new Dictionary<T, T?>(
            dictionary,
            ReferenceEqualityComparer<T>.Instance);
    }

    public static HashSet<T> IntersectFrozenSets<T>(IEnumerable<FrozenSet<T>> frozenSets)
    {
        var sets = frozenSets
            .OrderBy(x => x.Count);

        using var enumerator = sets.GetEnumerator();

        if (!enumerator.MoveNext())
            return [];

        var output = new HashSet<T>(enumerator.Current);

        while (enumerator.MoveNext())
        {
            output.IntersectWith(enumerator.Current);

            if (output.Count == 0)
                break;
        }

        return output;
    }
}