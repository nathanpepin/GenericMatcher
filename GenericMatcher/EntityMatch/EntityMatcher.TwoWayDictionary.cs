using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        params TMatchType[] requirements)
    {
        return CreateTwoWayMatchDictionaryInternal(otherEntities, requirements, false);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateStrictTwoWayMatchDictionary(
        TEntity[] otherEntities,
        params TMatchType[] requirements)
    {
        return CreateTwoWayMatchDictionaryInternal(otherEntities, requirements, true);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        params TMatchType[][] tieredCriteria)
    {
        var remaining = GetSeededHashSetFromPool(otherEntities);
        var otherToSeed = GetSeededDictionaryFromPool(otherEntities);
        var seedToOther = GetDictionaryFromPool(_dictionaryCache.Count);

        var hasDuplicates = false;

        try
        {
            foreach (var requirements in tieredCriteria)
            {
                if (remaining.Count == 0) break;

                var matches = FindMatchesForBatch(remaining, requirements, false);

                try
                {
                    ProcessMatches(matches, remaining, otherToSeed, ref hasDuplicates);
                }
                finally
                {
                    ReturnDictionaryToPool(matches);
                }
            }


            BuildReverseMapping(otherToSeed, seedToOther);

            var output = new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed, hasDuplicates);
            return output;
        }
        finally
        {
            ReturnHashSetToPool(remaining);
            ReturnDictionaryToPool(seedToOther);
            ReturnDictionaryToPool(otherToSeed);
        }
    }

    private TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionaryInternal(
        TEntity[] otherEntities,
        TMatchType[] requirements,
        bool strict)
    {
        var remaining = GetSeededHashSetFromPool(otherEntities);
        var otherToSeed = GetSeededDictionaryFromPool(otherEntities);
        var seedToOther = GetDictionaryFromPool(_dictionaryCache.Count);
        var hasDuplicates = false;

        try
        {
            while (remaining.Count > 0)
            {
                var previousCount = remaining.Count;

                var matches = FindMatchesForBatch(remaining, requirements, strict);

                try
                {
                    ProcessMatches(matches, remaining, otherToSeed, ref hasDuplicates);
                }
                finally
                {
                    ReturnDictionaryToPool(matches);
                }

                if (previousCount == remaining.Count) break;
            }

            BuildReverseMapping(otherToSeed, seedToOther);

            return new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed, hasDuplicates);
        }
        finally
        {
            ReturnHashSetToPool(remaining);
            ReturnDictionaryToPool(seedToOther);
            ReturnDictionaryToPool(otherToSeed);
        }
    }

    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> FindMatchesForBatch(
        HashSet<TEntity> entities,
        TMatchType[] requirements,
        bool strict)
    {
        var matchLookup = GetDictionaryFromPool(entities.Count);

        foreach (var entity in entities)
        {
            var matches = FindMatches(entity, requirements);
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

    private static Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetDictionaryFromPool(int count)
    {
        return DictionaryPool<TEntity, MatchingResult<TEntity, TMatchType>>.Get(count);
    }

    private static Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetSeededDictionaryFromPool(TEntity[] entities)
    {
        var output = DictionaryPool<TEntity, MatchingResult<TEntity, TMatchType>>.Get(entities.Length);

        foreach (var entity in entities) output[entity] = MatchingResult<TEntity, TMatchType>.Empty;

        return output;
    }

    private static HashSet<TEntity> GetHashSetFromPool(int length)
    {
        return HashSetPool<TEntity>.Get(length);
    }

    private static HashSet<TEntity> GetSeededHashSetFromPool(TEntity[] entities)
    {
        var output = HashSetPool<TEntity>.Get(entities.Length);

        foreach (var entity in entities) output.Add(entity);

        return output;
    }

    private static void ReturnDictionaryToPool(Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> dictionary)
    {
        DictionaryPool<TEntity, MatchingResult<TEntity, TMatchType>>.Return(dictionary);
    }

    private static void ReturnHashSetToPool(HashSet<TEntity> hashSet)
    {
        HashSetPool<TEntity>.Return(hashSet);
    }
}