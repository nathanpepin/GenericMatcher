using System.Collections.Frozen;
using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities,
        params IEnumerable<TMatchType> requirements)
    {
        return CreateTwoWayMatchDictionaryInternal(otherEntities, requirements, strict: false);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateStrictTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities,
        params IEnumerable<TMatchType> requirements)
    {
        return CreateTwoWayMatchDictionaryInternal(otherEntities, requirements, strict: true);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities,
        params IEnumerable<TMatchType[]> tieredCriteria)
    {
        var remaining = new HashSet<TEntity>(otherEntities);
        var otherToSeed = otherEntities.ToNullMatchingResults<TEntity, TMatchType>();
        var seedToOther = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var hasDuplicates = false;

        foreach (var requirements in tieredCriteria)
        {
            if (remaining.Count == 0) break;

            var matches = FindMatchesForBatch(remaining, requirements, strict: false);
            ProcessMatches(matches, remaining, otherToSeed, ref hasDuplicates);
        }

        BuildReverseMapping(otherToSeed, seedToOther);

        return new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed, hasDuplicates);
    }

    private TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionaryInternal(
        IEnumerable<TEntity> otherEntities,
        IEnumerable<TMatchType> requirements,
        bool strict)
    {
        var remaining = new HashSet<TEntity>(otherEntities);
        var otherToSeed = otherEntities.ToNullMatchingResults<TEntity, TMatchType>();
        var seedToOther = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var hasDuplicates = false;

        while (remaining.Count > 0)
        {
            var previousCount = remaining.Count;
            var matches = FindMatchesForBatch(remaining, requirements, strict);

            ProcessMatches(matches, remaining, otherToSeed, ref hasDuplicates);

            // If no new matches were found in this iteration, break
            if (previousCount == remaining.Count) break;
        }

        BuildReverseMapping(otherToSeed, seedToOther);

        return new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed, hasDuplicates);
    }

    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> FindMatchesForBatch(
        HashSet<TEntity> entities,
        IEnumerable<TMatchType> requirements,
        bool strict)
    {
        var matchLookup = new Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>(entities.Count);

        foreach (var entity in entities)
        {
            var matches = FindMatches(entity, requirements.ToArray());
            var result = matches switch
            {
                [] => MatchingResult<TEntity, TMatchType>.Empty,
                [var single] when entities.Contains(single) => new MatchingResult<TEntity, TMatchType>(single, requirements, false),
                [var single] => new MatchingResult<TEntity, TMatchType>(single, requirements, true),
                [..] when strict => throw new DuplicateKeyException(),
                [..] when matches.ToArray().FirstOrDefault(entities.Contains) is { } match => new MatchingResult<TEntity, TMatchType>(match, requirements,
                    true),
                _ => MatchingResult<TEntity, TMatchType>.Empty
            };

            matchLookup[entity] = result;
        }

        return matchLookup;
    }

    private static void ProcessMatches(
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> matches,
        HashSet<TEntity> remaining,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed,
        ref bool hasDuplicates)
    {
        foreach (var (entity, result) in matches)
        {
            if (result.Match == null) continue;

            remaining.Remove(result.Match);
            otherToSeed[entity] = result;
            hasDuplicates |= result.IsDuplicate;
        }
    }

    private static void BuildReverseMapping(
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther)
    {
        foreach (var (key, value) in otherToSeed)
        {
            if (value.Match is null) continue;

            var reverseMatch = new MatchingResult<TEntity, TMatchType>(key, value.Requirements, value.IsDuplicate);
            seedToOther[value.Match] = reverseMatch;
        }
    }
}