using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    public FrozenDictionary<TEntity, TEntity[]> CreateMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType> requirements)
    {
        var otherEntitiesMaterialized = otherEntities.ToArray().AsSpan();
        var requirementsMaterialized = requirements.ToArray().AsSpan();

        var otherToSeed = new Dictionary<TEntity, TEntity[]>(otherEntitiesMaterialized.Length, ReferenceEqualityComparer<TEntity>.Instance);

        for (var i = 0; i < otherEntitiesMaterialized.Length; i++)
        {
            var other = otherEntitiesMaterialized[i];
            var matches = FindMatches(other, requirementsMaterialized);

            if (matches.Length == 0)
            {
                otherToSeed.TryAdd(other, []);
                continue;
            }

            otherToSeed.TryAdd(other, [..matches]);
        }

        return otherToSeed.ToFrozenDictionary();
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType> requirements)
    {
        var otherToSeed = CreateMatchDictionary(otherEntities, requirements)
            .ToFrozenDictionary(x => x.Key, v => v.Value.FirstOrDefault());

        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var (key, values) in otherToSeed)
        {
            if (values is null) continue;

            seedToOther[values] = key;
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateStrictTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType> requirements)
    {
        var otherToSeed = CreateMatchDictionary(otherEntities, requirements)
            .ToFrozenDictionary(
                x => x.Key,
                v => v.Value.Length > 1
                    ? throw new MoreThanOneMatchException()
                    : v.Value.FirstOrDefault());

        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var (key, values) in otherToSeed)
        {
            if (values is null) continue;

            seedToOther[values] = key;
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType[]> tieredCriteria)
    {
        var otherEntitiesMaterialized = otherEntities as TEntity[] ?? otherEntities.ToArray();
        var otherToSeed = otherEntitiesMaterialized.ToNullDictionary();

        HashSet<TEntity> remaining = [..otherEntitiesMaterialized];
        HashSet<TEntity> found = [];
        HashSet<TEntity> toRemove = [];

        foreach (var requirement in tieredCriteria)
        {
            foreach (var item in remaining)
            {
                var matches = FindMatches(item, requirement);

                var availableMatches = found.ExceptBySpan([..matches]);

                switch (availableMatches)
                {
                    case 0:
                        continue;
                    case [var match]:
                        toRemove.Add(item);
                        found.Add(match);
                        break;
                    case [var match, ..]:
                        toRemove.Add(item);
                        found.Add(match);
                        break;
                }
            }

            foreach (var remove in toRemove)
            {
                remaining.Remove(remove);
            }

            toRemove.Clear();
        }

        var seedToOther = new Dictionary<TEntity, TEntity?>(_dictionaryCache, ReferenceEqualityComparer<TEntity>.Instance);

        foreach (var (key, values) in otherToSeed)
        {
            if (values is null) continue;

            seedToOther[values] = key;
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    public TwoWayFrozenMatchDictionary<TEntity> CreateStrictTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, IEnumerable<TMatchType[]> tieredCriteria,
        ParallelOptions? parallelOptions = null)
    {
        return CreateTwoWayMatchDictionaryTieredBase([..otherEntities], [..tieredCriteria], true);
    }
}