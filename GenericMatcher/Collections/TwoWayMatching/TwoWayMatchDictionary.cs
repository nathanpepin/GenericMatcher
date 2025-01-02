using System.Collections.Immutable;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections.TwoWayMatching;

public readonly struct TwoWayMatchDictionary<TEntity, TMatchType> : ITwoWayMatchDictionary<TEntity, TMatchType> where TEntity : class
    where TMatchType : struct, Enum
{
    public TwoWayMatchDictionary(Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther,
        Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed)
    {
        SeedToOther = seedToOther;
        OtherToSeed = otherToSeed;

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
        return SeedToOther[entity];
    }

    public MatchingResult<TEntity, TMatchType> FindMatchFromOtherToSeed(TEntity entity)
    {
        return OtherToSeed[entity];
    }

    public bool HasMatch(TEntity entity)
    {
        return OtherToSeed[entity].Match is not null;
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

    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> SeedToOther { get; }
    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> OtherToSeed { get; }

    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _seedToOtherImmutable;
    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _otherToSeedImmutable;
    private readonly Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> _matchedSeedToOther;

    private readonly Lazy<ImmutableHashSet<TEntity>> _unmatchedFromSeed;
    private readonly Lazy<ImmutableHashSet<TEntity>> _unmatchedFromOther;
}