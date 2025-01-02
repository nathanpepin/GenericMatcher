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

        GetSeedToOtherDictionary = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(seedToOther.ToImmutableDictionary);
        GetOtherToSeedDictionary = new Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>>(otherToSeed.ToImmutableDictionary);

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

    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> MatchedSeedToOther { get; }
    public Lazy<ImmutableHashSet<TEntity>> UnMatchedFromSeed { get; }
    public Lazy<ImmutableHashSet<TEntity>> UnMatchedFromOther { get; }
    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> GetSeedToOtherDictionary { get; }
    public Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> GetOtherToSeedDictionary { get; }

    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> SeedToOther { get; }
    private Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> OtherToSeed { get; }
}