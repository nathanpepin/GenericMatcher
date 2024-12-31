using System.Buffers;
using System.Runtime.CompilerServices;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity? FindFirstMatchOrDefault(TEntity entity, ReadOnlySpan<TMatchType> matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return FindMatches(entity, matchRequirements) switch
        {
            [var match] => match,
            [var match, ..] => match,
            _ => null
        };
    }

    public ReadOnlySpan<TEntity> FindMatches(TEntity entity, ReadOnlySpan<TMatchType> matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);

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
            var currentMatches = strategies[matchRequirements[i]].GetMatches(entity);
            if (currentMatches.Length == 0)
                return [];

            var tempArray = ArrayPool<TEntity>.Shared.Rent(seedEntities.Length);
            var tempSpan = tempArray.AsSpan();
            var matchCount = 0;

            try
            {
                for (var j = 0; j < seedEntities.Length; j++)
                {
                    var seedEntity = seedEntities[j];
                    if (currentMatches.Contains(seedEntity))
                        tempSpan[matchCount++] = seedEntity;
                }

                if (matchCount == 0)
                    return [];

                seedEntities = tempSpan[..matchCount];
            }
            finally
            {
                ArrayPool<TEntity>.Shared.Return(tempArray);
            }
        }

        return seedEntities;
    }
}

file static class Extensions
{
    public static bool Contains<TEntity>(this ReadOnlySpan<TEntity> span, TEntity entity)
    {
        for (var i = 0; i < span.Length; i++)
        {
            var iEntity = span[i];
            if (Equals(iEntity, entity)) return true;
        }

        return false;
    }
}