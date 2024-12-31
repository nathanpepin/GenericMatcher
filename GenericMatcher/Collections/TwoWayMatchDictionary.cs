using System.Collections.Frozen;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Collections;

/// <summary>
///     A wrapper over two dictionaries that represent a two-way dictionary.
/// </summary>
/// <param name="aToB"></param>
/// <param name="bToA"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TMatchType"></typeparam>
public readonly struct TwoWayMatchDictionary<TEntity, TMatchType>(
    Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> aToB,
    Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> bToA)
    where TEntity : class
    where TMatchType : struct, Enum
{
    public Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> AToB { get; } = aToB;
    public Dictionary<TEntity, MatchingResult<TEntity, TMatchType>> BToA { get; } = bToA;

    public Lazy<Dictionary<TEntity, MatchingResult<TEntity, TMatchType>>> AToBMatchedResults { get; } = new(() => aToB
        .Where(x => x.Value.Match is not null)
        .ToDictionary());

    public Lazy<HashSet<TEntity>> AToBUnmatchedResults { get; } = new(() => aToB
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToHashSet());

    public Lazy<HashSet<TEntity>> BToAUnmatchedResults { get; } = new(bToA
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToHashSet());
}