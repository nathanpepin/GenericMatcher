using System.Runtime.CompilerServices;
using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        TMatchType[] criteria,
        bool throwOnDuplicateMatch = true)
    {
        ArgumentNullException.ThrowIfNull(otherEntities);
        ArgumentNullException.ThrowIfNull(criteria);

        return CreateTwoWayMatchDictionary(otherEntities, [criteria], throwOnDuplicateMatch);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        TMatchType[][] tieredCriteria,
        bool throwOnDuplicateMatch = true)
    {
        ArgumentNullException.ThrowIfNull(otherEntities);
        ArgumentNullException.ThrowIfNull(tieredCriteria);

        if (tieredCriteria.Length == 0)
            throw new ArgumentException("Tiered criteria cannot be empty", nameof(tieredCriteria));

        using var remainingInOther = new PooledHashSet<TEntity>(otherEntities.Length);
        using var remainingInSeed = new PooledHashSet<TEntity>(_seedEntities.Count);
        
        using var otherToSeed = new PooledDictionary<TEntity, MatchingResult<TEntity, TMatchType>>(otherEntities.Length);
        using var seedToOther = new PooledDictionary<TEntity, MatchingResult<TEntity, TMatchType>>(_seedEntities.Count);

        remainingInOther.HashSet.UnionWith(otherEntities);
        remainingInSeed.HashSet.UnionWith(_seedEntities);

        foreach (var entity in otherEntities)
            otherToSeed.Dictionary[entity] = MatchingResult<TEntity, TMatchType>.Empty;
        foreach (var entity in _seedEntities)
            seedToOther.Dictionary[entity] = MatchingResult<TEntity, TMatchType>.Empty;

        foreach (var tier in tieredCriteria)
        {
            if (remainingInOther.HashSet.Count == 0) break;

            ProcessTier(
                tier,
                remainingInOther.HashSet,
                remainingInSeed.HashSet,
                otherToSeed.Dictionary,
                seedToOther.Dictionary,
                throwOnDuplicateMatch);
        }

        return new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(
            seedToOther.Dictionary,
            otherToSeed.Dictionary);
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
                throw new DuplicateKeyException();

            var match = reducedMatches[0];

            remainingInOther.Remove(entity);
            remainingInSeed.Remove(match);
            
            otherToSeed[entity] = new MatchingResult<TEntity, TMatchType>(match, tier);
            seedToOther[match] = new MatchingResult<TEntity, TMatchType>(entity, tier);
        }
    }

    private static ReadOnlySpan<TEntity> ReduceMatchesFromRemaining(ReadOnlySpan<TEntity> matches, HashSet<TEntity> remaining)
    {
        Span<TEntity> result = new TEntity[matches.Length];
        var index = 0;

        foreach (var match in matches)
        {
            if (!remaining.Contains(match)) continue;

            result[index] = match;
            index++;
        }

        return result[..index];
    }
}