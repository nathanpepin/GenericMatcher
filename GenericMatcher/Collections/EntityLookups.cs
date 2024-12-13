using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher.Collections;

/// <summary>
///     A wrapper type over a dictionary that groups properties to their match types
/// </summary>
/// <param name="it"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TMatchType"></typeparam>
public sealed class EntityLookups<TEntity, TMatchType>(FrozenDictionary<TMatchType, FrozenDictionary<object, ImmutableHashSet<TEntity>>> it)
    where TEntity : notnull
    where TMatchType : Enum
{
    public FrozenDictionary<TMatchType, FrozenDictionary<object, ImmutableHashSet<TEntity>>> It { get; } = it;
}