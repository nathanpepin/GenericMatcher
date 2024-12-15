using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;

namespace GenericMatcher.EntityMatch;

public sealed partial class EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : Enum
{
    public FrozenSet<TEntity> FindMatches(TEntity entity, params TMatchType[] matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(matchRequirements);

        if (matchRequirements.Length == 0)
            return FrozenSet<TEntity>.Empty;

        // ReSharper disable once InlineTemporaryVariable
        var strategies = _matchStrategies;

        var seedEntities = strategies[matchRequirements[0]].GetMatches(entity);

        if (seedEntities.Count == 0)
            return FrozenSet<TEntity>.Empty;

        HashSet<TEntity> found = [..seedEntities];

        for (var i = 1; i < matchRequirements.Length; i++)
        {
            var entities = strategies[matchRequirements[i]].GetMatches(entity);
            if (entities.Count == 0)
                return FrozenSet<TEntity>.Empty;
            
            found.IntersectWith(entities);

            if (found.Count == 0)
                return FrozenSet<TEntity>.Empty;
        }

        return [..found];
    }
    
    public MatchResult<TEntity, TMatchType> FindMatchesTiered(TEntity entity,
        params TMatchType[][] matchRequirementGroupings)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(matchRequirementGroupings);

        if (matchRequirementGroupings.Length == 0)
        {
            return MatchResult<TEntity, TMatchType>.Empty;
        }

        foreach (var matchRequirements in matchRequirementGroupings)
        {
            var matches = FindMatches(entity, matchRequirements);
            if (matches.Count > 0) continue;

            return new MatchResult<TEntity, TMatchType>
            {
                Matches = matches,
                MatchRequirementMet = matchRequirements
            };
        }

        return MatchResult<TEntity, TMatchType>.Empty;
    }
}