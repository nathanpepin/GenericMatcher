// using System.Collections.Immutable;
//
// namespace GenericMatcher.EntityMatch;
//
// /// <summary>
// /// Provides a builder to create an <see cref="EntityMatcher{TEntity, TMatchType}"/> instance.
// /// This class allows the configuration of seed entities and match definitions that are used
// /// to evaluate entity matches based on specified criteria.
// /// </summary>
// /// <typeparam name="TEntity">
// /// The type of the entity to be matched. Must be a reference type.
// /// </typeparam>
// /// <typeparam name="TMatchType">
// /// The type of the match criteria. Must be an enum.
// /// </typeparam>
// public sealed class EntityMatcherBuilder<TEntity, TMatchType>
//     where TEntity : class
//     where TMatchType : Enum
// {
//     /// <summary>
//     /// Represents a collection of match definitions used to configure the entity matcher.
//     /// </summary>
//     /// <remarks>
//     /// The collection stores instances of <see cref="MatchDefinition{TEntity, TMatchType}"/>
//     /// that specify how entities will be grouped or matched. This is an internal property
//     /// used during the construction of the entity matcher.
//     /// </remarks>
//     private readonly List<MatchDefinition<TEntity, TMatchType>> _definitions = [];
//     
//     private readonly HashSet<TMatchType> _usedMatchTypes = [];
//
//     /// <summary>
//     /// A private field that stores an immutable hash set of seed entities used
//     /// for initializing the entity matching process in the builder.
//     /// This field is set using the <c>WithSeedEntities</c> method and is required
//     /// for constructing an <c>EntityMatcher</c> instance.
//     /// </summary>
//     private readonly HashSet<TEntity> _seedEntities = [];
//
//     /// <summary>
//     /// Specifies a set of seed entities to be used as the foundation for building entity matches.
//     /// </summary>
//     /// <param name="entities">The collection of entities to be used as seed entities.</param>
//     /// <returns>Returns the current instance of <see cref="EntityMatcherBuilder{TEntity, TMatchType}"/> to allow for method chaining.</returns>
//     public EntityMatcherBuilder<TEntity, TMatchType> WithSeedEntities(IEnumerable<TEntity> entities)
//     {
//         _seedEntities.UnionWith(entities);
//
//         return this;
//     }
//
//     /// <summary>
//     /// Adds a match definition to the entity matcher by associating a match type with a key selector function.
//     /// </summary>
//     /// <param name="matchType">The matching criterion type, which must be unique within the builder's definitions.</param>
//     /// <param name="keySelector">A function that extracts a key from an entity for matching purposes.</param>
//     /// <returns>Returns the current instance of <see cref="EntityMatcherBuilder{TEntity, TMatchType}"/> to enable method chaining.</returns>
//     public EntityMatcherBuilder<TEntity, TMatchType> AddMatchDefinition(
//         TMatchType matchType,
//         Func<TEntity, object> keySelector)
//     {
//         if (!_usedMatchTypes.Add(matchType))
//         {
//             throw new ArgumentException($"Cannot use more than one match definition per match type: {matchType}", nameof(matchType));
//         }
//
//         _definitions.Add(new MatchDefinition<TEntity, TMatchType>(matchType, keySelector));
//
//         return this;
//     }
//
//     /// Builds an instance of EntityMatcher with the configured seed entities and match definitions.
//     /// This method ensures that all seed entities and match definitions have been provided before
//     /// constructing the EntityMatcher instance. Throws an exception if the seed entities are null or not configured.
//     /// <return>
//     /// An instance of EntityMatcher configured with the provided seed entities and match definitions.
//     /// </return>
//     public EntityMatcher<TEntity, TMatchType> Build()
//     {
//         ArgumentNullException.ThrowIfNull(_seedEntities);
//         return new EntityMatcher<TEntity, TMatchType>(_seedEntities, _definitions);
//     }
// }