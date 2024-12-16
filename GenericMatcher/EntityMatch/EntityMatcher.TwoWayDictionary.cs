using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType> requirements)
    {
        return CreateTwoWayMatchDictionaryBase([..otherEntities], [..requirements], false);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateStrictTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType> requirements)
    {
        return CreateTwoWayMatchDictionaryBase([..otherEntities], [..requirements], true);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<IEnumerable<TMatchType>> tieredCriteria)
    {
        return CreateTwoWayMatchDictionaryTieredBase([..otherEntities], [..tieredCriteria.Select(x => x.ToArray())], false);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateStrictTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<IEnumerable<TMatchType>> tieredCriteria,
        ParallelOptions? parallelOptions = null)
    {
        return CreateTwoWayMatchDictionaryTieredBase([..otherEntities], [..tieredCriteria.Select(x => x.ToArray())], true);
    }

    private TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionaryBase(HashSet<TEntity> otherEntities, TMatchType[] requirements, bool errorIfDuplicate)
    {
        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var otherToSeed = new Dictionary<TEntity, TEntity?>(otherEntities.Count, ReferenceEqualityComparer<TEntity>.Instance);

        ProcessMatches(otherEntities, requirements, seedToOther, otherToSeed, errorIfDuplicate);

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    private TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionaryTieredBase(HashSet<TEntity> otherEntities, TMatchType[][] tieredCriteria, bool errorIfDuplicate)
    {
        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var otherToSeed = new Dictionary<TEntity, TEntity?>(otherEntities.Count, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var requirements in tieredCriteria)
        {
            foreach (var (key, value) in otherToSeed)
            {
                otherEntities.Remove(key);
            }

            ProcessMatches(otherEntities, requirements, seedToOther, otherToSeed, errorIfDuplicate);
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }


    private void ProcessMatches(HashSet<TEntity> entities,
        TMatchType[] criteria,
        Dictionary<TEntity, TEntity?> seedToOther,
        Dictionary<TEntity, TEntity?> otherToSeed, bool errorIfDuplicate)
    {
        foreach (var entity in entities)
        {
            var match = errorIfDuplicate
                ? FindMatches(entity, criteria).SingleOrDefault()
                : FindMatches(entity, criteria).FirstOrDefault();

            otherToSeed.Add(entity, match);

            if (match != null)
            {
                seedToOther.TryAdd(match, entity);
            }
        }
    }
}