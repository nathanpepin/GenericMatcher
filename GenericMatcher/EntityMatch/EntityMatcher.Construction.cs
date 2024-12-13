using System.Collections.Frozen;
using System.Collections.Immutable;
using GenericMatcher.Collections;

namespace GenericMatcher.EntityMatch;

public sealed partial class EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : Enum
{
    /// Represents a utility for matching entities based on defined criteria and match definitions.
    /// This class is designed to be generic, working with any entity type (`TEntity`) and match type (`TMatchType`).
    /// Type Parameters:
    /// TEntity:
    /// The type of the entity being matched. It must be a reference type.
    /// TMatchType:
    /// The type representing match criteria. It must be an enumeration.
    /// Examples of use include scenarios like matching records based on specific attributes,
    /// such as identifying duplicates or correlating entities from different datasets.
    /// The class uses immutable collections for storing input entities and match definitions,
    /// ensuring thread safety and immutability once initialized.
    /// Features include:
    /// - Accepting initial seed entities and match definitions upon construction.
    /// - Efficiently building lookup structures for entity matching.
    /// - Providing mechanisms for entity matching and creating two-way mappings.
    /// Members of this class are divided across partial implementations, ensuring modularity.
    public EntityMatcher(
        IEnumerable<TEntity> seedEntities,
        IEnumerable<MatchDefinition<TEntity, TMatchType>> matchDefinitions)
    {
        ArgumentNullException.ThrowIfNull(seedEntities);
        ArgumentNullException.ThrowIfNull(matchDefinitions);

        _seedEntities = seedEntities.ToImmutableHashSet();
        var definitions = matchDefinitions.ToImmutableArray();

        var entityLookups = CreateLookups(_seedEntities, definitions);
        _matchStrategies = CreateMatchStrategies(definitions, entityLookups);
    }

    /// <summary>
    /// Creates lookup structures for entities based on match definitions. Each match definition specifies
    /// a match type and a key selector used to group entities.
    /// </summary>
    /// <param name="entities">
    /// A collection of entities that will be grouped using the match definitions.
    /// </param>
    /// <param name="definitions">
    /// A collection of match definitions, where each definition includes a match type and a key selector
    /// function.
    /// </param>
    /// <returns>
    /// An instance of <see cref="EntityLookups{TEntity, TMatchType}"/> containing the grouped entities
    /// organized by match types.
    /// </returns>
    private static EntityLookups<TEntity, TMatchType> CreateLookups(
        ImmutableHashSet<TEntity> entities,
        ImmutableArray<MatchDefinition<TEntity, TMatchType>> definitions)
    {
        var lookups = definitions
            .ToFrozenDictionary(
                definition => definition.MatchType,
                definition => CreateLookup(entities, definition.KeySelector));

        return new EntityLookups<TEntity, TMatchType>(lookups);
    }

    /// <summary>
    /// Creates a lookup data structure that groups entities by a specified key.
    /// </summary>
    /// <param name="entities">The collection of entities to be grouped.</param>
    /// <param name="keySelector">A function to extract the key for each entity.</param>
    /// <returns>A frozen dictionary that maps keys to immutable hash sets of entities.</returns>
    private static FrozenDictionary<object, ImmutableHashSet<TEntity>> CreateLookup(
        ImmutableHashSet<TEntity> entities,
        Func<TEntity, object> keySelector)
    {
        return entities
            .GroupBy(keySelector)
            .ToFrozenDictionary(
                group => group.Key, group => group.ToImmutableHashSet());
    }

    /// Creates match strategies based on the given match definitions and entity lookups.
    /// <param name="definitions">
    /// A collection of match definitions, each containing a match type and a key selector function for the entities.
    /// </param>
    /// <param name="lookups">
    /// A dictionary that maps match types to their respective lookups, which associate keys to sets of entities.
    /// </param>
    /// <returns>
    /// A MatchStrategies object containing strategies for matching entities based on match types.
    /// </returns>
    private static MatchStrategies<TEntity, TMatchType> CreateMatchStrategies(
        ImmutableArray<MatchDefinition<TEntity, TMatchType>> definitions,
        EntityLookups<TEntity, TMatchType> lookups)
    {
        var strategies = definitions.ToFrozenDictionary(
            definition => definition.MatchType,
            definition => new Func<TEntity, ImmutableHashSet<TEntity>>(entity =>
                GetFromLookup(lookups.It[definition.MatchType], definition.KeySelector(entity))));

        return new MatchStrategies<TEntity, TMatchType>(strategies);
    }

    /// <summary>
    /// Retrieves a set of matches for a given key from a lookup dictionary.
    /// </summary>
    /// <param name="lookup">The lookup dictionary containing keys and their corresponding sets of entities.</param>
    /// <param name="key">The key to search for in the lookup dictionary.</param>
    /// <returns>
    /// A set of entities associated with the specified key in the lookup dictionary.
    /// If the key is not found, an empty set is returned.
    /// </returns>
    private static ImmutableHashSet<TEntity> GetFromLookup(
        FrozenDictionary<object, ImmutableHashSet<TEntity>> lookup,
        object key)
    {
        return lookup.TryGetValue(key, out var matches) ? matches : ImmutableHashSet<TEntity>.Empty;
    }
}