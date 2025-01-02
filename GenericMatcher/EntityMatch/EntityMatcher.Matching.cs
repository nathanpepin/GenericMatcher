using System.Buffers;
using System.Runtime.CompilerServices;
using GenericMatcher.Collections;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity? FindFirstMatchOrDefault(TEntity entity, ReadOnlySpan<TMatchType> matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return FindMatches(entity, matchRequirements) switch
        {
            [var match] => match,
            [var match, ..] => match,
            _ => null
        };
    }

    public ReadOnlySpan<TEntity> FindMatches(TEntity entity, ReadOnlySpan<TMatchType> matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (matchRequirements.Length == 0)
            return [];

        var strategies = _matchStrategies;
        var seedEntities = strategies[matchRequirements[0]]
            .GetMatches(entity)
            .ToArray()
            .AsSpan();

        if (seedEntities.Length == 0)
            return [];

        for (var i = 1; i < matchRequirements.Length; i++)
        {
            var currentMatches = strategies[matchRequirements[i]].GetMatches(entity);
            if (currentMatches.Length == 0)
                return [];

            var tempArray = CustomArrayPool<TEntity>.Rent(seedEntities.Length);
            var tempSpan = tempArray.AsSpan();
            var matchCount = 0;

            try
            {
                for (var j = 0; j < seedEntities.Length; j++)
                {
                    var seedEntity = seedEntities[j];
                    if (currentMatches.Contains(seedEntity))
                        tempSpan[matchCount++] = seedEntity;
                }

                if (matchCount == 0)
                    return [];

                seedEntities = tempSpan[..matchCount];
            }
            finally
            {
                CustomArrayPool<TEntity>.Return(tempArray);
            }
        }

        return seedEntities;
    }

    public ReadOnlySpan<TEntity> FindMatchesParallel(TEntity entity, ReadOnlySpan<TMatchType> matchRequirements, ParallelOptions parallelOptions)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (matchRequirements.Length == 0)
            return [];

        var strategies = _matchStrategies;
        var seedEntitiesArray = strategies[matchRequirements[0]]
            .GetMatches(entity)
            .ToArray();

        if (seedEntitiesArray.Length == 0)
            return [];

        const int parallelThreshold = 100;

        for (var i = 1; i < matchRequirements.Length; i++)
        {
            var currentMatches = strategies[matchRequirements[i]].GetMatches(entity);
            if (currentMatches.Length == 0)
                return [];

            var currentMatchesArray = CustomArrayPool<TEntity>.Rent(currentMatches.Length);
            currentMatches.CopyTo(currentMatchesArray);

            var tempArray = CustomArrayPool<TEntity>.Rent(seedEntitiesArray.Length);
            var matchCount = 0;

#if NET9_0_OR_GREATER
            var currentLock = _lock;
#else
            var currentLock = new object();
#endif

            try
            {
                if (seedEntitiesArray.Length >= parallelThreshold)
                {
                    var batches = CreateBatches(seedEntitiesArray.Length, Environment.ProcessorCount);

                    var array = seedEntitiesArray;
                    Parallel.ForEach(batches, parallelOptions, batch =>
                    {
                        var localTemp = new List<TEntity>();

                        for (var j = batch.Start; j < batch.End; j++)
                        {
                            if (currentMatchesArray.Contains(array[j]))
                            {
                                localTemp.Add(array[j]);
                            }
                        }

                        if (localTemp.Count <= 0) return;

                        lock (currentLock)
                        {
                            localTemp.CopyTo(tempArray, matchCount);
                            matchCount += localTemp.Count;
                        }
                    });
                }
                else
                {
                    for (var j = 0; j < seedEntitiesArray.Length; j++)
                    {
                        if (currentMatches.Contains(seedEntitiesArray[j]))
                        {
                            tempArray[matchCount++] = seedEntitiesArray[j];
                        }
                    }
                }

                if (matchCount == 0)
                    return [];

                seedEntitiesArray = tempArray[..matchCount].ToArray();
            }
            finally
            {
                CustomArrayPool<TEntity>.Return(currentMatchesArray);
                CustomArrayPool<TEntity>.Return(tempArray);
            }
        }

        return seedEntitiesArray;
    }

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#endif

    private readonly record struct Batch(int Start, int End);

    private static IEnumerable<Batch> CreateBatches(int totalSize, int batchCount)
    {
        var batchSize = Math.Max(1, totalSize / batchCount);

        for (var i = 0; i < totalSize; i += batchSize)
        {
            yield return new Batch(i, Math.Min(i + batchSize, totalSize));
        }
    }
}

file static class Extensions
{
    public static bool Contains<TEntity>(this ReadOnlySpan<TEntity> span, TEntity entity)
    {
        for (var i = 0; i < span.Length; i++)
        {
            var iEntity = span[i];
            if (Equals(iEntity, entity)) return true;
        }

        return false;
    }
}