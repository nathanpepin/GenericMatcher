using System.Collections.Immutable;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections.TwoWayMatching;

public interface ITwoWayMatchDictionary<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    MatchingResult<TEntity, TMatchType> FindMatchFromSeedToOther(TEntity entity);
    MatchingResult<TEntity, TMatchType> FindMatchFromOtherToSeed(TEntity entity);
    bool HasMatch(TEntity entity);
    ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> MatchedSeedToOther();
    ImmutableHashSet<TEntity> UnMatchedFromSeed(TEntity entity);
    ImmutableHashSet<TEntity> UnMatchedFromOther(TEntity entity);
    ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetSeedToOtherDictionary();
    ImmutableDictionary<TEntity, MatchingResult<TEntity, TMatchType>> GetOtherToSeedDictionary();
}