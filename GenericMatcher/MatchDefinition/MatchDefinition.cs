using System.Collections.Frozen;
using GenericMatcher.Exceptions;

namespace GenericMatcher.MatchDefinition;

public abstract class MatchDefinition<TEntity, TMatchType, TProperty> : IMatchDefinition<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : Enum
    where TProperty : notnull
{
    public bool IsSeeded { get; private set; }

    private ReadOnlyMemory<TEntity> Entities { get; set; }
    public FrozenDictionary<TProperty, FrozenSet<TEntity>>? EntityDictionary { get; private set; }

    public abstract Func<TEntity, TProperty> Conversion { get; }

    public void Seed(TEntity[] entities)
    {
        Entities = entities;
        EntityDictionary = null;
        IsSeeded = true;
    }

    public abstract TMatchType MatchType { get; }

    public FrozenSet<TEntity> GetMatches(TEntity entity)
    {
        if (!IsSeeded)
            throw new MatchDefinitionNotSeededException();

        var key = Conversion(entity);

        return GetEntityDictionary()
            .TryGetValue(key, out var matches)
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
            if (!EntitiesMatch(firstEntity, enumerator.Current))
                return false;

        return true;
    }

    private FrozenDictionary<TProperty, FrozenSet<TEntity>> GetEntityDictionary()
    {
        if (EntityDictionary is not null) return EntityDictionary;

        var dictionary = new Dictionary<TProperty, HashSet<TEntity>>(Entities.Length);

        var entitiesSpan = Entities.Span;

        for (var i = 0; i < Entities.Length; i++)
        {
            var it = entitiesSpan[i];
            var property = Conversion(it);

            if (dictionary.TryGetValue(property, out var value))
                value.Add(it);
            else
                dictionary.Add(property, [it]);
        }

        return EntityDictionary = dictionary
            .ToFrozenDictionary(x => x.Key, x => x.Value.ToFrozenSet());
    }
}