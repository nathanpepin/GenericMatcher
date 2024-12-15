using System.Collections.Frozen;
using System.Collections.Immutable;
using GenericMatcher.Exceptions;

namespace GenericMatcher;

public interface IMatchDefinition<TEntity, out TMatchType>
    where TEntity : class where TMatchType : Enum
{
    void Seed(FrozenSet<TEntity> entities);

    TMatchType MatchType { get; }

    FrozenSet<TEntity> GetMatches(TEntity entity);

    bool EntitiesMatch(TEntity a, TEntity b);

    bool AllEntitiesMatch(params IEnumerable<TEntity> entities);
}

public interface IMatchDefinitionString<in TEntity>
{
    Func<TEntity, ReadOnlySpan<char>> ConvertToSpan { get; }
}

public abstract class MatchDefinition<TEntity, TMatchType, TProperty> : IMatchDefinition<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : Enum
    where TProperty : notnull
{
    public bool IsSeeded { get; private set; }

    public void Seed(FrozenSet<TEntity> entities)
    {
        Entities = entities;
        EntityDictionary = null;
        IsSeeded = true;
    }

    private FrozenSet<TEntity> Entities { get; set; } = [];
    public abstract TMatchType MatchType { get; }
    public FrozenDictionary<TProperty, FrozenSet<TEntity>>? EntityDictionary { get; private set; }

    public abstract Func<TEntity, TProperty> Conversion { get; }

    public FrozenSet<TEntity> GetMatches(TEntity entity)
    {
        if (!IsSeeded)
            throw new MatchDefinitionNotSeededException();

        EntityDictionary ??= Entities
            .AsParallel()
            .GroupBy(x => Conversion(x))
            .ToFrozenDictionary(x => x.Key, x => x.ToFrozenSet());

        var key = Conversion(entity);
        return EntityDictionary.TryGetValue(key, out var matches)
            ? matches
            : FrozenSet<TEntity>.Empty;
    }

    public bool EntitiesMatch(TEntity a, TEntity b)
    {
        return Conversion(a).Equals(Conversion(b));
    }

    public bool AllEntitiesMatch(params IEnumerable<TEntity> entities)
    {
        if (!IsSeeded)
            throw new MatchDefinitionNotSeededException();

        using var enumerator = entities.GetEnumerator();

        if (!enumerator.MoveNext())
            return true;

        var firstEntity = enumerator.Current;

        while (enumerator.MoveNext())
        {
            if (!EntitiesMatch(firstEntity, enumerator.Current))
                return false;
        }

        return true;
    }
}