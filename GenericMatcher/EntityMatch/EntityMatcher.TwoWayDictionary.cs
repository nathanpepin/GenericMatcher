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
    public JsonSerializerOptions JsonOptions { get; init; } = new() { WriteIndented = true };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ITwoWayMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        TMatchType[] criteria,
        ParallelOptions? parallelOptions = null,
        bool throwOnDuplicateMatch = true)
    {
        ArgumentNullException.ThrowIfNull(otherEntities);
        ArgumentNullException.ThrowIfNull(criteria);

        return CreateTwoWayMatchDictionary(otherEntities, [criteria], parallelOptions, throwOnDuplicateMatch);
    }

    public ITwoWayMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        TMatchType[][] tieredCriteria,
        ParallelOptions? parallelOptions = null,
        bool throwOnDuplicateMatch = true)
    {
        ArgumentNullException.ThrowIfNull(otherEntities);
        ArgumentNullException.ThrowIfNull(tieredCriteria);

        if (tieredCriteria.Length == 0)
            throw new ArgumentException("Tiered criteria cannot be empty", nameof(tieredCriteria));

        if (parallelOptions is not null && otherEntities.Length >= ParallelStart)
        {
            return CreateTwoWayMatchDictionaryParallel(otherEntities, tieredCriteria, parallelOptions, throwOnDuplicateMatch);
        }

        using var remainingInOther = new PooledHashSet<TEntity>(otherEntities.Length);
        remainingInOther.HashSet.UnionWith(otherEntities);

        using var remainingInSeed = new PooledHashSet<TEntity>(_seedEntities.Length);
        remainingInSeed.HashSet.UnionWith(_seedEntities);

        var otherToSeed = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(otherEntities.Length);
        var seedToOther = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(_seedEntities.Length);

        foreach (var tier in tieredCriteria)
        {
            if (remainingInOther.HashSet.Count == 0) break;

            ProcessTier(
                tier,
                remainingInOther.HashSet,
                remainingInSeed.HashSet,
                otherToSeed,
                seedToOther,
                throwOnDuplicateMatch);
        }

        foreach (var entity in remainingInOther.HashSet)
        {
            otherToSeed[entity] = MatchingResult<TEntity, TMatchType>.Empty;
        }

        foreach (var entity in remainingInSeed.HashSet)
        {
            seedToOther[entity] = MatchingResult<TEntity, TMatchType>.Empty;
        }

        return new TwoWayMatchDictionary<TEntity, TMatchType>(
            seedToOther,
            otherToSeed);
    }

    private void ProcessTier(
        TMatchType[] tier,
        HashSet<TEntity> remainingInOther,
        HashSet<TEntity> remainingInSeed,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther,
        bool throwOnDuplicateMatch)
    {
        foreach (var entity in remainingInOther)
        {
            var matches = FindMatches(entity, tier);
            var reducedMatches = ReduceMatchesFromRemaining(matches, remainingInSeed);

            if (reducedMatches.Length == 0)
                continue;

            if (throwOnDuplicateMatch && reducedMatches.Length > 1)
            {
                var entityJson = JsonSerializer.Serialize(entity, JsonOptions);
                var entitiesJson = JsonSerializer.Serialize(reducedMatches.ToArray(), JsonOptions);
                var tierJson = JsonSerializer.Serialize(tier, JsonOptions);
                throw new DuplicateKeyException(entityJson, entitiesJson, tierJson);
            }

            var match = reducedMatches[0];

            remainingInOther.Remove(entity);
            remainingInSeed.Remove(match);

            otherToSeed[entity] = new MatchingResult<TEntity, TMatchType>(match, tier);
            seedToOther[match] = new MatchingResult<TEntity, TMatchType>(entity, tier);
        }
    }

    private static ReadOnlySpan<TEntity> ReduceMatchesFromRemaining(ReadOnlySpan<TEntity> matches, HashSet<TEntity> remaining)
    {
        var rentedArray = CustomArrayPool<TEntity>.Rent(matches.Length);
        var rentedSpan = rentedArray.AsSpan();
        var index = 0;

        try
        {
            foreach (var match in matches)
            {
                if (remaining.Contains(match))
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