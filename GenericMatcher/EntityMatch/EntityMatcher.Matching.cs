using System.Collections.Immutable;

namespace GenericMatcher.EntityMatch;

public sealed partial class EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : Enum
{
    /// <summary>
    /// Finds and returns the entities that match the specified entity based on the given match requirements.
    /// </summary>
    /// <param name="entity">The entity to find matches for.</param>
    /// <param name="matchRequirements">A collection of match requirements specifying the criteria for matching.</param>
    /// <returns>
    /// An immutable array of entities that match the criteria defined by the given match requirements.
    /// If no match requirements are provided or no matches are found, an empty immutable array is returned.
    /// </returns>
    public ImmutableArray<TEntity> FindMatches(TEntity entity, params IEnumerable<TMatchType> matchRequirements)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(matchRequirements);

        var matchRequirementMaterialized = matchRequirements.ToImmutableArray();

        if (!matchRequirementMaterialized.Any())
            return ImmutableArray<TEntity>.Empty;

        return
        [
            ..matchRequirementMaterialized
                .Select(type => GetStrategy(type)(entity))
                .Aggregate((current, next) => current.Intersect(next))
        ];
    }

    /// <summary>
    /// Asynchronously finds and returns the entities that match the specified entity based on the given match requirements.
    /// </summary>
    /// <param name="entity">The entity to find matches for.</param>
    /// <param name="matchRequirements">A collection of match requirements specifying the criteria for matching.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an immutable array of entities
    /// that match the criteria defined by the given match requirements. If no match requirements are provided or
    /// no matches are found, an empty immutable array is returned.
    /// </returns>
    public async Task<ImmutableArray<TEntity>> FindMatchesAsync(
        TEntity entity,
        IEnumerable<TMatchType> matchRequirements,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var matchRequirementMaterialized = matchRequirements.ToImmutableArray();

        if (!matchRequirementMaterialized.Any())
            return ImmutableArray<TEntity>.Empty;

        var matchTasks = matchRequirementMaterialized
            .Select(type => Task.Run(() => GetStrategy(type)(entity), cancellationToken));

        var results = await Task.WhenAll(matchTasks);

        return [..results.Aggregate((current, next) => current.Intersect(next))];
    }

    /// <summary>
    /// Finds matches for a given entity based on a tiered hierarchy of match requirements.
    /// The method evaluates each requirement grouping sequentially and returns the matches
    /// from the first tier that satisfies the conditions.
    /// </summary>
    /// <param name="entity">The entity to find matches for. Cannot be null.</param>
    /// <param name="matchRequirementGroupings">
    /// A collection of tiered match requirement groupings, where each grouping contains
    /// match requirements at that specific tier.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// 1. An immutable array of matching entities if a match is found in any tier; otherwise, an empty array.
    /// 2. An immutable array of match requirements from the successful tier if a match is found; otherwise, an empty array.
    /// </returns>
    public (ImmutableArray<TEntity>, ImmutableArray<TMatchType>) FindMatchesTiered(TEntity entity,
        params IEnumerable<IEnumerable<TMatchType>> matchRequirementGroupings)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(matchRequirementGroupings);

        var matchRequirementGroupingsMaterialized = matchRequirementGroupings
            .Select(x => x.ToImmutableArray())
            .ToImmutableArray();

        if (!matchRequirementGroupingsMaterialized.Any())
            return (ImmutableArray<TEntity>.Empty, ImmutableArray<TMatchType>.Empty);


        foreach (var matchRequirements in matchRequirementGroupingsMaterialized)
        {
            var matches = FindMatches(entity, matchRequirements);
            if (matches.Length == 0) continue;

            return (matches, [..matchRequirements]);
        }

        return (ImmutableArray<TEntity>.Empty, ImmutableArray<TMatchType>.Empty);
    }

    /// <summary>
    /// Asynchronously finds and returns the entities that match the specified entity based on tiered match requirements.
    /// </summary>
    /// <param name="entity">The entity to find tiered matches for.</param>
    /// <param name="matchRequirementGroupings">
    /// A collection of collections, where each inner collection represents a set of match requirements to evaluate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A tuple where the first item is an immutable array of the entities that match the criteria of any tier, and the second item is an immutable array of match requirements corresponding to the matches.
    /// If no match groupings or matches are found, both items in the tuple will be empty immutable arrays.
    /// </returns>
    public async Task<(ImmutableArray<TEntity>, ImmutableArray<TMatchType>)> FindMatchesTieredAsync(
        TEntity entity,
        IEnumerable<IEnumerable<TMatchType>> matchRequirementGroupings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var matchRequirementGroupingsMaterialized = matchRequirementGroupings
            .Select(x => x.ToImmutableArray())
            .ToImmutableArray();


        if (!matchRequirementGroupingsMaterialized.Any())
            return (ImmutableArray<TEntity>.Empty, ImmutableArray<TMatchType>.Empty);

        foreach (var matchRequirements in matchRequirementGroupingsMaterialized)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var matches = await FindMatchesAsync(entity, matchRequirements, cancellationToken);
            if (matches.Length == 0) continue;

            return (matches, [..matchRequirements]);
        }

        return (ImmutableArray<TEntity>.Empty, ImmutableArray<TMatchType>.Empty);
    }

    /// <summary>
    /// Retrieves the matching strategy associated with the specified match type.
    /// </summary>
    /// <param name="matchType">The match type for which the strategy is to be retrieved.</param>
    /// <returns>
    /// A function that, when given an entity of type <typeparamref name="TEntity"/>, returns an
    /// <see cref="ImmutableHashSet{TEntity}"/> of matching entities.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when there is no strategy defined for the specified <paramref name="matchType"/>.
    /// </exception>
    private Func<TEntity, ImmutableHashSet<TEntity>> GetStrategy(TMatchType matchType)
    {
        return _matchStrategies.Value.It.TryGetValue(matchType, out var strategy)
            ? strategy
            : throw new ArgumentException($"No strategy defined for match type: {matchType}", nameof(matchType));
    }
}