using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher.MatchDefinition;

public interface IMatchDefinition<TEntity, out TMatchType>
    where TEntity : class where TMatchType : Enum
{
    TMatchType MatchType { get; }
    void Seed(TEntity[] entities);
    ReadOnlySpan<TEntity> GetMatches(TEntity entity);
}