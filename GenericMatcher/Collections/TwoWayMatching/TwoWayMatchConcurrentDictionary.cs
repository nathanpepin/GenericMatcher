using System.Collections.Concurrent;
using System.Collections.Immutable;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections.TwoWayMatching;

public readonly struct TwoWayMatchConcurrentDictionary<TEntity, TMatchType> : ITwoWayMatchDictionary<TEntity, TMatchType> where TEntity : class
    where TMatchType : struct, Enum
{
    public TwoWayMatchConcurrentDictionary(ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther,
        ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed)
    {
        _seedToOtherConcurrent = seedToOther;
        _otherToSeedConcurrent = otherToSeed;

        SeedToOtherDictionary = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(seedToOther.ToImmutableDictionary);
        OtherToSeedDictionary = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(otherToSeed.ToImmutableDictionary);


        MatchedSeedToOther = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(() => seedToOther
            .Where(x => x.Value.Match is not null)
            .ToImmutableDictionary(x => x.Key, x => x.Value));

        UnMatchedFromSeed = new Lazy<ImmutableHashSet<TEntity>>(() =>
        [
            ..seedToOther
                .Where(x => x.Value.Match is null)
                .Select(x => x.Key)
        ]);

        UnMatchedFromOther = new Lazy<ImmutableHashSet<TEntity>>(() =>
        [
            ..otherToSeed
                .Where(x => x.Value.Match is null)
                .Select(x => x.Key)
        ]);
    }

    public MatchingResult<TEntity, TMatchType> FindMatchFromSeedToOther(TEntity entity)
    {
        return _seedToOtherConcurrent[entity];
    }

    public MatchingResult<TEntity, TMatchType> FindMatchFromOtherToSeed(TEntity entity)
    {
        return _otherToSeedConcurrent[entity];
    }

    public bool HasMatch(TEntity entity)
    {
        return _otherToSeedConcurrent[entity].Match is not null;
    }

    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> MatchedSeedToOther { get; }
    public Lazy<ImmutableHashSet<TEntity>> UnMatchedFromSeed { get; }
    public Lazy<ImmutableHashSet<TEntity>> UnMatchedFromOther { get; }
    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> SeedToOtherDictionary { get; }
    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> OtherToSeedDictionary { get; }


    private readonly ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> _seedToOtherConcurrent;
    private readonly ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> _otherToSeedConcurrent;
}