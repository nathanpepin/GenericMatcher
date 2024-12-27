using System.Runtime.CompilerServices;

namespace GenericMatcher.Collections;

public sealed class HashSetPool<T>
{
    private static readonly ObjectPool<HashSet<T>> Pool;

    static HashSetPool()
    {
        Pool = new ObjectPool<HashSet<T>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashSet<T> Get(int? capacity = null, IEqualityComparer<T>? comparer = null)
    {
        var hashSet = Pool.Get();
        hashSet.Clear();

        if ((!capacity.HasValue || hashSet.EnsureCapacity(capacity.Value) >= capacity.Value)
            && (comparer == null || Equals(hashSet.Comparer, comparer))) return hashSet;

        Pool.Return(hashSet);
        return new HashSet<T>(capacity ?? 0, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(HashSet<T> hashSet)
    {
        if (hashSet == null!) return;

        hashSet.Clear();
        Pool.Return(hashSet);
    }
}