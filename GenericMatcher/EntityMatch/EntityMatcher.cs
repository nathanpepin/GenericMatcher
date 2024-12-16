using System.Collections.Frozen;
using System.Collections.Immutable;
using GenericMatcher.Collections;
using GenericMatcher.MatchDefinition;

namespace GenericMatcher.EntityMatch;

/// <summary>
/// Represents a generic mechanism for matching entities based on configurable match definitions.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be matched. Must be a reference type.</typeparam>
/// <typeparam name="TMatchType">The type of the match criteria. Must be an enum.</typeparam>
public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    /// <summary>
    /// Represents a lazy readonly instance of mapping strategies used for entity matching.
    /// Encapsulates the logic required to determine relationships between entities
    /// and their corresponding match types within a given context.
    /// </summary>
    private readonly FrozenDictionary<TMatchType, IMatchDefinition<TEntity, TMatchType>> _matchStrategies;

    /// <summary>
    /// A collection of seed entities that serves as the primary dataset for entity matching operations.
    /// </summary>
    /// <remarks>
    /// This collection is immutable and initialized during the construction of the <see cref="EntityMatcher{TEntity, TMatchType}"/>.
    /// It is used as the baseline for matching other entities and constructing lookup dictionaries.
    /// </remarks>
    private readonly FrozenSet<TEntity> _seedEntities;

    private readonly ImmutableDictionary<TEntity, TEntity?> _dictionaryCache;
}