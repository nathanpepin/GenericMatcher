using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using GenericMatcher.Exceptions;

namespace GenericMatcher.MatchDefinition;

public abstract class MatchDefinition<TEntity, TMatchType, TProperty> : IMatchDefinition<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : Enum
    where TProperty : notnull
{
    protected virtual StringComparison StringComparison => StringComparison.OrdinalIgnoreCase;

    public bool IsSeeded { get; private set; }
    private ReadOnlyMemory<TEntity> Entities { get; set; }
    public Dictionary<TProperty, ReadOnlyMemory<TEntity>>? EntityDictionary { get; private set; }

    public abstract Func<TEntity, TProperty> Conversion { get; }

    public virtual Func<TEntity, bool> FilterMatch { get; } = _ => true;

    public void Seed(TEntity[] entities)
    {
        Entities = entities;
        EntityDictionary = null;
        IsSeeded = true;
    }

    public abstract TMatchType MatchType { get; }

    public ReadOnlySpan<TEntity> GetMatches(TEntity entity)
    {
        if (!IsSeeded)
        {
            throw new MatchDefinitionNotSeededException();
        }

        var key = Conversion(entity);

        if (FilterMatch(entity) is false)
        {
            return [];
        }

        return GetEntityDictionary()
            .TryGetValue(key, out var matches)
            ? matches.Span
            : [];
    }

    public bool EntitiesMatch(TEntity a, TEntity b)
    {
        return Conversion(a).Equals(Conversion(b));
    }

    public bool AllEntitiesMatch(IEnumerable<TEntity> entities)
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

    private Dictionary<TProperty, ReadOnlyMemory<TEntity>> GetEntityDictionary()
    {
        if (EntityDictionary is not null) return EntityDictionary;

        var equalityComparer = typeof(TProperty) == typeof(string)
            ? (IEqualityComparer<TProperty>)StringComparer.FromComparison(StringComparison)
            : EqualityComparer<TProperty>.Default;

        var dictionary = new Dictionary<TProperty, HashSet<TEntity>>(
            Entities.Length,
            equalityComparer);

        var span = Entities.Span;
        for (var i = 0; i < span.Length; i++)
        {
            var entity = span[i];
            var key = Conversion(entity);
            ref var bucket = ref CollectionsMarshal.GetValueRefOrAddDefault(
                dictionary, key, out var exists);
            if (!exists) bucket = [];
            bucket!.Add(entity);
        }

        return EntityDictionary = dictionary.ToDictionary(
            x => x.Key,
            x => new ReadOnlyMemory<TEntity>(x.Value.ToArray()));
    }
}