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
        return CreateTwoWayMatchDictionaryInternal(otherEntities, requirements);
    }

    public TwoWayFrozenMatchDictionary<TEntity, TMatchType> CreateTwoWayMatchDictionary(
        TEntity[] otherEntities,
        params TMatchType[][] tieredCriteria)
    {
        var remaining = GetSeededHashSetFromPool(otherEntities);
        var otherToSeed = GetSeededDictionaryFromPool(otherEntities);
        var seedToOther = GetSeededDictionaryFromPool(_dictionaryCache.Keys.ToArray());

        try
        {
            foreach (var requirements in tieredCriteria)
            {
                if (remaining.Count == 0) break;

                var matches = FindMatchesForBatch(remaining, requirements);

                try
                {
                    ProcessMatches(matches, remaining, otherToSeed);
                }
                finally
                {
                    ReturnDictionaryToPool(matches);
                }
            }

            BuildReverseMapping(otherToSeed, seedToOther);

            var output = new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed);
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
        TMatchType[] requirements)
    {
        var remaining = GetSeededHashSetFromPool(otherEntities);
        var otherToSeed = GetSeededDictionaryFromPool(otherEntities);
        var seedToOther = GetSeededDictionaryFromPool(_dictionaryCache.Keys.ToArray());

        try
        {
            while (remaining.Count > 0)
            {
                var previousCount = remaining.Count;

                var matches = FindMatchesForBatch(remaining, requirements);

                try
                {
                    ProcessMatches(matches, remaining, otherToSeed);
                }
                finally
                {
                    ReturnDictionaryToPool(matches);
                }

                if (previousCount == remaining.Count) break;
            }

            BuildReverseMapping(otherToSeed, seedToOther);

            return new TwoWayFrozenMatchDictionary<TEntity, TMatchType>(seedToOther, otherToSeed);
        }
        finally
        {
            ReturnHashSetToPool(remaining);
            ReturnDictionaryToPool(seedToOther);
            ReturnDictionaryToPool(otherToSeed);
        }
    }

    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> FindMatchesForBatch(
        HashSet<TEntity> remaining,
        TMatchType[] requirements)
    {
        var matchLookup = GetDictionaryFromPool(remaining.Count);

        foreach (var entity in remaining)
        {
            var matches = FindMatches(entity, requirements);

            MatchingResult<TEntity, TMatchType> result;

            switch (matches)
            {
                case []:
                    result = MatchingResult<TEntity, TMatchType>.Empty;
                    break;
                case [var single] when remaining.Contains(single):
                    remaining.Remove(single);
                    result = new MatchingResult<TEntity, TMatchType>(single, requirements);
                    break;
                case [var single] when !remaining.Contains(single):
                    result = MatchingResult<TEntity, TMatchType>.Empty;
                    break;
                case [..] when matches.ToArray().FirstOrDefault(remaining.Contains) is { } match:
                    remaining.Remove(match);
                    result = new MatchingResult<TEntity, TMatchType>(match, requirements);
                    break;
                default:
                    result = MatchingResult<TEntity, TMatchType>.Empty;
                    break;
            }

            matchLookup[entity] = result;
        }

        return matchLookup;
    }

    private static void ProcessMatches(
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> matches,
        HashSet<TEntity> remaining,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed)
    {
        foreach (var (entity, result) in matches)
        {
            if (result.Match == null) continue;

            remaining.Remove(result.Match);
            otherToSeed[entity] = result;
        }
    }

    private static void BuildReverseMapping(
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther)
    {
        foreach (var (key, value) in otherToSeed)
        {
            if (value.Match is null) continue;

            var reverseMatch = new MatchingResult<TEntity, TMatchType>(key, value.Requirements);
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