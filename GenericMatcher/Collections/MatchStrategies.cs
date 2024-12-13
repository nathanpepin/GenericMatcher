using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher.Collections;

/// <summary>
///     A wrapper type that allows a lookup using an entity as a key
/// </summary>
/// <param name="it"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TMatchType"></typeparam>
internal sealed class MatchStrategies<TEntity, TMatchType>(FrozenDictionary<TMatchType, Func<TEntity, ImmutableHashSet<TEntity>>> it)
    where TEntity : notnull
    where TMatchType : Enum
{
    public FrozenDictionary<TMatchType, Func<TEntity, ImmutableHashSet<TEntity>>> It { get; } = it;
}