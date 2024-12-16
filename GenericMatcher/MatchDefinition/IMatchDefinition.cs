using System.Collections.Frozen;

namespace GenericMatcher.MatchDefinition;

public interface IMatchDefinition<TEntity, out TMatchType>
    where TEntity : class where TMatchType : Enum
{
    void Seed(FrozenSet<TEntity> entities);

    TMatchType MatchType { get; }

    FrozenSet<TEntity> GetMatches(TEntity entity);

    bool EntitiesMatch(TEntity a, TEntity b);

    bool AllEntitiesMatch(params IEnumerable<TEntity> entities);
}