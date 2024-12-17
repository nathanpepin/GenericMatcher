using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using GenericMatcher.Collections;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    public TEntity[] FindMatches(TEntity entity, params ReadOnlySpan<TMatchType> matchRequirements)
    {
        if (matchRequirements.Length == 0)
            return [];

        var strategies = _matchStrategies;

        var seedEntities = strategies[matchRequirements[0]]
            .GetMatches(entity)
            .ToArray()
            .AsSpan();

        if (seedEntities.Length == 0)
            return [];

        for (var i = 1; i < matchRequirements.Length; i++)
        {
            var entities = strategies[matchRequirements[i]].GetMatches(entity);
            if (entities.Count == 0)
                return [];

            Span<TEntity> found = new TEntity [seedEntities.Length];
            var foundIndex = 0;

            for (var j = 0; j < seedEntities.Length; j++)
            {
                var seedEntity = seedEntities[j];

                if (!entities.Contains(seedEntity)) continue;

                found[foundIndex++] = seedEntity;
            }

            if (found.Length == 0)
                return [];

            seedEntities = found[..foundIndex];
        }

        return [..seedEntities];
    }

    public MatchResult<TEntity, TMatchType> FindMatchesTiered(TEntity entity,
        params ReadOnlySpan<TMatchType[]> matchRequirementGroupings)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (matchRequirementGroupings.Length == 0)
        {
            return MatchResult<TEntity, TMatchType>.Empty;
        }

        foreach (var matchRequirements in matchRequirementGroupings)
        {
            var matches = FindMatches(entity, matchRequirements);
            if (matches.Length > 0) continue;

            return new MatchResult<TEntity, TMatchType>(matches, matchRequirements);
        }

        return MatchResult<TEntity, TMatchType>.Empty;
    }
}