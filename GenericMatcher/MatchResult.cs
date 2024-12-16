using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher;

public readonly struct MatchResult<TEntity, TMatchType>(FrozenSet<TEntity> matches, TMatchType[] requirements)
    where TEntity : class
    where TMatchType : struct, Enum
{
    public FrozenSet<TEntity> Matches { get; } = matches;
    public TMatchType[] MatchRequirementsMet { get; } = requirements;
    public bool FoundMatches => Matches.Count == 0;

    public static MatchResult<TEntity, TMatchType> Empty => new(FrozenSet<TEntity>.Empty, []);
}

public sealed class EntityLookups<TEntity, TMatchType>
    where TEntity : notnull
    where TMatchType : struct, Enum // Constrain to value type
{
    private readonly ImmutableDictionary<TMatchType, ILookup<object, TEntity>> _lookups;

    public EntityLookups(IEnumerable<TEntity> entities,
        IReadOnlyDictionary<TMatchType, Func<TEntity, object>> converters)
    {
        _lookups = converters.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => entities.ToLookup(kvp.Value)
        );
    }

    public IEnumerable<TEntity> FindMatches(TEntity entity,
        TMatchType matchType,
        Func<TEntity, object> converter) =>
        _lookups[matchType][converter(entity)];
}

public interface ISpanMatchDefinition<in TEntity>
{
    ReadOnlySpan<char> GetSpan(TEntity entity);
}

public sealed class StringMatchDefinition<TEntity, TMatchType>(
    TMatchType matchType,
    Func<TEntity, string> converter,
    Func<TEntity, ReadOnlySpan<char>> spanConverter)
    : MatchDefinition<TEntity, TMatchType, string>, ISpanMatchDefinition<TEntity>
    where TEntity : class
    where TMatchType : struct, Enum
{
    public ReadOnlySpan<char> GetSpan(TEntity entity) => spanConverter(entity);
    public override TMatchType MatchType { get; } = matchType;
    public override Func<TEntity, string> Conversion { get; } = converter;
}