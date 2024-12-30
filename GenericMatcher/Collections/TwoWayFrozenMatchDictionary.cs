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
public readonly struct TwoWayFrozenMatchDictionary<TEntity, TMatchType>(
    IDictionary<TEntity, MatchingResult<TEntity, TMatchType>> aToB,
    IDictionary<TEntity, MatchingResult<TEntity, TMatchType>> bToA)
    where TEntity : class
    where TMatchType : struct, Enum
{
    public FrozenDictionary<TEntity, MatchingResult<TEntity, TMatchType>> AToB { get; } = aToB.ToFrozenDictionary();
    public FrozenDictionary<TEntity, MatchingResult<TEntity, TMatchType>> BToA { get; } = bToA.ToFrozenDictionary();

    public FrozenDictionary<TEntity, MatchingResult<TEntity, TMatchType>> AToBMatchedResults { get; } = aToB
        .Where(x => x.Value.Match is not null)
        .ToFrozenDictionary();

    public FrozenDictionary<TEntity, MatchingResult<TEntity, TMatchType>> BToAMatchedResults { get; } = bToA
        .Where(x => x.Value.Match is not null)
        .ToFrozenDictionary();

    public FrozenSet<TEntity> AToBUnmatchedResults { get; } = aToB
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToFrozenSet();

    public FrozenSet<TEntity> BToAUnmatchedResults { get; } = bToA
        .Where(x => x.Value.Match is null)
        .Select(x => x.Key)
        .ToFrozenSet();
    public TEntity? GetMatchFromEither(TEntity key)
    {
        return AToB.TryGetValue(key, out var value) ? value : BToA.TryGetValue(key, out value) ? value : default;
    }

    public bool HasMatch(TEntity key)
    {
        return AToB.ContainsKey(key) && BToA.ContainsKey(key);
    }
}