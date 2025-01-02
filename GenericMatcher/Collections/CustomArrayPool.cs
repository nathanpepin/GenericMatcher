using System.Buffers;

namespace GenericMatcher.Collections;

internal static class CustomArrayPool<TEntity>
    where TEntity : class
{
    private static readonly ArrayPool<TEntity> ArrayPool =
        ArrayPool<TEntity>.Create(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 50);

    public static TEntity[] Rent(int length)
    {
        return ArrayPool.Rent(Math.Min(length, 1024 * 1024));
    }

    public static void Return(TEntity[] entities)
    {
        ArrayPool.Return(entities);
    }
}