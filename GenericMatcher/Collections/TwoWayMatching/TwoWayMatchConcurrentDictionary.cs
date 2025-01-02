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

        _seedToOtherImmutable = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(seedToOther.ToImmutableDictionary);
        _otherToSeedImmutable = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(otherToSeed.ToImmutableDictionary);


        _matchedSeedToOther = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(() => seedToOther
            .Where(x => x.Value.Match is not null)
            .ToImmutableDictionary(x => x.Key, x => x.Value));

        _unmatchedFromSeed = new Lazy<ImmutableHashSet<TEntity>>(() =>
        [
            ..seedToOther
                .Where(x => x.Value.Match is null)
                .Select(x => x.Key)
        ]);

        _unmatchedFromOther = new Lazy<ImmutableHashSet<TEntity>>(() =>
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

    public ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> MatchedSeedToOther()
    {
        return _matchedSeedToOther.Value;
    }

    public ImmutableHashSet<TEntity> UnMatchedFromSeed(TEntity entity)
    {
        return _unmatchedFromSeed.Value;
    }

    public ImmutableHashSet<TEntity> UnMatchedFromOther(TEntity entity)
    {
        return _unmatchedFromOther.Value;
    }

    public ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetSeedToOtherDictionary() => _seedToOtherImmutable.Value;

    public ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetOtherToSeedDictionary() => _otherToSeedImmutable.Value;

    private readonly ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> _seedToOtherConcurrent;
    private readonly ConcurrentDictionary<TEntity, MatchingResult<TEntity, TMatchType>> _otherToSeedConcurrent;

    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _seedToOtherImmutable;
    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _otherToSeedImmutable;
    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _matchedSeedToOther;

    private readonly Lazy<ImmutableHashSet<TEntity>> _unmatchedFromSeed;
    private readonly Lazy<ImmutableHashSet<TEntity>> _unmatchedFromOther;
}