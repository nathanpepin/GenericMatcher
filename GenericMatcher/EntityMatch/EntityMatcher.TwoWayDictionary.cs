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

    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType[]> tieredCriteria)
    {
        return CreateTwoWayMatchDictionaryTieredBase([..otherEntities], [..tieredCriteria], false);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateStrictTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType[]> tieredCriteria,
        ParallelOptions? parallelOptions = null)
    {
        return CreateTwoWayMatchDictionaryTieredBase([..otherEntities], [..tieredCriteria], true);
    }

    private TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionaryBase(ReadOnlySpan<TEntity> otherEntities, ReadOnlySpan<TMatchType> requirements, bool errorIfDuplicate)
    {
        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var otherToSeed = new Dictionary<TEntity, TEntity?>(otherEntities.Length, ReferenceEqualityComparer<TEntity>.Instance);

        ProcessMatches(otherEntities, requirements, otherToSeed, errorIfDuplicate);

        foreach (var (key, value) in otherToSeed)
        {
            if (value is null) continue;
            seedToOther[value] = key;
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    private TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionaryTieredBase(ReadOnlySpan<TEntity> otherEntities, ReadOnlySpan<TMatchType[]> tieredCriteria, bool errorIfDuplicate)
    {
        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);
        var otherToSeed = new Dictionary<TEntity, TEntity?>(otherEntities.Length, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var requirements in tieredCriteria)
        {
            otherEntities = ProcessMatchesReturnUnmatched(otherEntities, requirements, otherToSeed, errorIfDuplicate);
        }

        foreach (var (key, value) in otherToSeed)
        {
            if (value is null) continue;
            seedToOther[value] = key;
        }
        
        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }


    private void ProcessMatches(ReadOnlySpan<TEntity> entities,
        ReadOnlySpan<TMatchType> criteria,
        Dictionary<TEntity, TEntity?> otherToSeed, bool errorIfDuplicate)
    {
        foreach (var entity in entities)
        {
            var match = errorIfDuplicate
                ? FindMatches(entity, criteria).SingleOrDefault()
                : FindMatches(entity, criteria).FirstOrDefault();

            otherToSeed.Add(entity, match);
        }
    }

    private ReadOnlySpan<TEntity> ProcessMatchesReturnUnmatched(ReadOnlySpan<TEntity> entities,
        ReadOnlySpan<TMatchType> criteria,
        Dictionary<TEntity, TEntity?> otherToSeed, bool errorIfDuplicate)
    {
        Span<TEntity> notFound = new TEntity [entities.Length];
        var notFoundIndex = 0;

        foreach (var entity in entities)
        {
            var match = errorIfDuplicate
                ? FindMatches(entity, criteria).SingleOrDefault()
                : FindMatches(entity, criteria).FirstOrDefault();

            otherToSeed.TryAdd(entity, match);

            if (match is null)
            {
                notFound[notFoundIndex++] = entity;
            }
        }

        return notFound[..notFoundIndex];
    }
}