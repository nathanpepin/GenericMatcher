using System.Collections.Frozen;

namespace GenericMatcher.MatchDefinition;

public interface IMatchDefinition<TEntity, out TMatchType>
    where TEntity : class where TMatchType : Enum
{
    TMatchType MatchType { get; }
    void Seed(TEntity[] entities);
    FrozenSet<TEntity> GetMatches(TEntity entity);
}