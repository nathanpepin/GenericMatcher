using System.Collections.Immutable;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections.TwoWayMatching;

public interface ITwoWayMatchDictionary<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    MatchingResult<TEntity, TMatchType> FindMatchFromSeedToOther(TEntity entity);
    MatchingResult<TEntity, TMatchType> FindMatchFromOtherToSeed(TEntity entity);
    bool HasMatch(TEntity entity);
    Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> MatchedSeedToOther { get; }
    Lazy<ImmutableHashSet<TEntity>> UnMatchedFromSeed { get; }
    Lazy<ImmutableHashSet<TEntity>> UnMatchedFromOther { get; }
    Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> GetSeedToOtherDictionary { get; }
    Lazy<ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>>> GetOtherToSeedDictionary { get; }
}