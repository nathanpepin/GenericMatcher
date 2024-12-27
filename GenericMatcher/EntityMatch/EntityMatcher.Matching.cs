using System.Buffers;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    public TEntity? FindFirstMatchOrDefault(TEntity entity, params ReadOnlySpan<TMatchType> matchRequirements)
    {
        return FindMatches(entity, matchRequirements) switch
        {
            [var match] => match,
            [var match, ..] => match,
            _ => null
        };
    }

    public ReadOnlySpan<TEntity> FindMatches(TEntity entity, params ReadOnlySpan<TMatchType> matchRequirements)
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

            var foundArray = ArrayPool<TEntity>.Shared.Rent(seedEntities.Length);
            var found = foundArray.AsSpan();

            try
            {
                var foundIndex = 0;

                foreach (var seedEntity in seedEntities)
                {
                    if (!entities.Contains(seedEntity)) continue;

                    found[foundIndex++] = seedEntity;
                }

                if (found.Length == 0)
                    return [];

                seedEntities = found[..foundIndex];
            }
            finally
            {
                ArrayPool<TEntity>.Shared.Return(foundArray);
            }
        }

        return seedEntities;
    }
}