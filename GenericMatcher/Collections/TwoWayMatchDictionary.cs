using System.Collections.Frozen;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections;

/// <summary>
///     A wrapper over two dictionaries that represent a two-way dictionary.
/// </summary>
/// <param name="seedToOther"></param>
/// <param name="otherToSeed"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TMatchType"></typeparam>
public readonly struct TwoWayMatchDictionary<TEntity, TMatchType>(
    Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> seedToOther,
    Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> otherToSeed)
    where TEntity : class
    where TMatchType : struct, Enum
{
    public Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> SeedToOther { get; } = seedToOther;
    public Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> OtherToSeed { get; } = otherToSeed;

    public Lazy<Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>> AToBMatchedResults { get; } = new(() => seedToOther
        .Where(x => x.Value.Match is not null)
        .ToDictionary());

    public Lazy<HashSet<TEntity>> AToBUnmatchedResults { get; } = new(() => seedToOther
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToHashSet());

    public Lazy<HashSet<TEntity>> BToAUnmatchedResults { get; } = new(otherToSeed
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToHashSet());
}