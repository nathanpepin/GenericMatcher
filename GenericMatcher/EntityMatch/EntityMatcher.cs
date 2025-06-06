using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using GenericMatcher.MatchDefinition;

namespace GenericMatcher.EntityMatch;

/// <summary>
///     Represents a generic mechanism for matching entities based on configurable match definitions.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be matched. Must be a reference type.</typeparam>
/// <typeparam name="TMatchType">The type of the match criteria. Must be an enum.</typeparam>
public readonly partial struct EntityMatcher<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    /// <summary>
    ///     Represents a lazy readonly instance of mapping strategies used for entity matching.
    ///     Encapsulates the logic required to determine relationships between entities
    ///     and their corresponding match types within a given context.
    /// </summary>
    private readonly FrozenDictionary<TMatchType, IMatchDefinition<TEntity, TMatchType>> _matchStrategies;

    private readonly TEntity[] _seedEntities;
}