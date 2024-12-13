using System.Collections.Immutable;
using GenericMatcher.Collections;

namespace GenericMatcher.EntityMatch;

public sealed partial class EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : Enum
{
    /// <summary>
    /// Creates a two-way mapping dictionary between seed entities and other entities based on match requirements.
    /// </summary>
    /// <param name="otherEntities">The entities to match against the seed entities.</param>
    /// <param name="requirements">The match criteria defining the matching rules.</param>
    /// <returns>A bidirectional frozen dictionary linking seed and other entities.</returns>
    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionary(
        IEnumerable<TEntity> otherEntities, params IEnumerable<TMatchType> requirements)
    {
        var seedToOther = CreateEntityToNullMapping(_seedEntities);
        var otherToSeed = CreateEntityToNullMapping(otherEntities);

        ProcessMatches(otherEntities, requirements, seedToOther, otherToSeed);

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    /// <summary>
    /// Creates a two-way mapping dictionary between seed and other entities based on tiered matching rules.
    /// </summary>
    /// <param name="otherEntities">The entities to match against the seed entities.</param>
    /// <param name="tieredCriteria">Tiers of match requirements for applying stepwise matching.</param>
    /// <returns>A bidirectional frozen dictionary linking seed and other entities.</returns>
    public TwoWayFrozenMatchDictionary<TEntity> CreateTwoWayMatchDictionaryTiered(
        IEnumerable<TEntity> otherEntities, IEnumerable<IEnumerable<TMatchType>> tieredCriteria)
    {
        var seedToOther = CreateEntityToNullMapping(_seedEntities);
        var otherToSeed = CreateEntityToNullMapping(otherEntities);

        foreach (var criteria in tieredCriteria)
        {
            var unmatchedEntities = otherToSeed
                .Where(pair => pair.Value == null)
                .Select(pair => pair.Key);

            ProcessMatches(unmatchedEntities, criteria, seedToOther, otherToSeed);
        }

        return new TwoWayFrozenMatchDictionary<TEntity>(seedToOther, otherToSeed);
    }

    /// <summary>
    /// Creates a dictionary mapping each entity to null initially.
    /// </summary>
    /// <param name="entities">The entities to include in the mapping.</param>
    /// <returns>A dictionary where keys are entities and values are initially null.</returns>
    private static Dictionary<TEntity, TEntity?> CreateEntityToNullMapping(IEnumerable<TEntity> entities)
    {
        return entities.ToDictionary(entity => entity, TEntity? (_) => null);
    }

    /// <summary>
    /// Processes matches between entities based on the specified match criteria and updates the mappings.
    /// </summary>
    /// <param name="entities">The entities to process matches for.</param>
    /// <param name="criteria">The matching criteria to apply.</param>
    /// <param name="seedToOther">Mapping from seeds to others.</param>
    /// <param name="otherToSeed">Mapping from others to seeds.</param>
    private void ProcessMatches(
        IEnumerable<TEntity> entities,
        IEnumerable<TMatchType> criteria,
        Dictionary<TEntity, TEntity?> seedToOther,
        Dictionary<TEntity, TEntity?> otherToSeed)
    {
        foreach (var entity in entities)
        {
            var match = FindMatches(entity, criteria).SingleOrDefault();
            otherToSeed[entity] = match;

            if (match != null)
            {
                seedToOther[match] = entity;
            }
        }
    }
}