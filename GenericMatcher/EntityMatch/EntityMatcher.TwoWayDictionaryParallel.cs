using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using GenericMatcher.Collections;
using GenericMatcher.Collections.TwoWayMatching;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    private const uint ParallelStart = 10_000;

    private ITwoWayMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionaryParallel(
        TEntity[] otherEntities,
        TMatchType[][] tieredCriteria,
        ParallelOptions parallelOptions,
        bool throwOnDuplicateMatch = true)
    {
        ArgumentNullException.ThrowIfNull(otherEntities);
        ArgumentNullException.ThrowIfNull(tieredCriteria);

        if (tieredCriteria.Length == 0)
            throw new ArgumentException("Tiered criteria cannot be empty", nameof(tieredCriteria));

        var foundInOther = new ConcurrentBag<TEntity>();
        var foundInSeed = new ConcurrentBag<TEntity>();

        var otherToSeed = new ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>>();
        Parallel.ForEach(otherEntities, parallelOptions, entity => otherToSeed[entity] = MatchingResult<TEntity, TMatchType>.Empty);

        var seedToOther = new ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>>();
        Parallel.ForEach(_seedEntities, parallelOptions, entity => seedToOther[entity] = MatchingResult<TEntity, TMatchType>.Empty);

        foreach (var tier in tieredCriteria)
        {
            if (foundInOther.IsEmpty) break;

            ProcessTierParallel(
                tier,
                foundInOther,
                foundInSeed,
                otherToSeed,
                seedToOther,
                throwOnDuplicateMatch,
                parallelOptions);
        }


        return new TwoWayMatchConcurrentDictionary<TEntity, TMatchType>(
            seedToOther,
            otherToSeed);
    }

    private void ProcessTierParallel(
        TMatchType[] tier,
        ConcurrentBag<TEntity> foundInOther,
        ConcurrentBag<TEntity> foundInSeed,
        ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed,
        ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther,
        bool throwOnDuplicateMatch,
        ParallelOptions parallelOptions)
    {
        var it = this;

        Parallel.ForEach(foundInOther, parallelOptions, entity =>
        {
            var matches = it.FindMatchesParallel(entity, tier, parallelOptions);
            var reducedMatches = ReduceMatchesFromRemainingParallel(matches, foundInSeed);

            if (reducedMatches.Length == 0)
                return;

            if (throwOnDuplicateMatch && reducedMatches.Length > 1)
            {
                var entityJson = JsonSerializer.Serialize(entity);
                var entitiesJson = JsonSerializer.Serialize(reducedMatches.ToArray());
                var tierJson = JsonSerializer.Serialize(tier);
                throw new DuplicateKeyException(entityJson, entitiesJson, tierJson);
            }

            var match = reducedMatches[0];

            foundInOther.Add(match);
            foundInSeed.Add(entity);

            otherToSeed[entity] = new MatchingResult<TEntity, TMatchType>(match, tier);
            seedToOther[match] = new MatchingResult<TEntity, TMatchType>(entity, tier);
        });
    }

    private static ReadOnlySpan<TEntity> ReduceMatchesFromRemainingParallel(ReadOnlySpan<TEntity> matches, ConcurrentBag<TEntity> found)
    {
        var rentedArray = CustomArrayPool<TEntity>.Rent(matches.Length);
        var rentedSpan = rentedArray.AsSpan();
        var index = 0;

        try
        {
            for (var i = 0; i < matches.Length; i++)
            {
                var match = matches[i];
                if (!found.Contains(match))
                    rentedSpan[index++] = match;
            }

            return rentedSpan[..index];
        }
        finally
        {
            CustomArrayPool<TEntity>.Return(rentedArray);
        }
    }
}