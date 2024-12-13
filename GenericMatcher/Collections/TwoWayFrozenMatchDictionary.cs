using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher.Collections;

/// <summary>
///     A wrapper over two dictionaries that represent a two-way dictionary.
/// </summary>
/// <param name="aToB"></param>
/// <param name="bToA"></param>
/// <typeparam name="TEntity"></typeparam>
public sealed class TwoWayFrozenMatchDictionary<TEntity>(IDictionary<TEntity, TEntity?> aToB, IDictionary<TEntity, TEntity?> bToA)
    where TEntity : notnull
{
    public FrozenDictionary<TEntity, TEntity?> AToB { get; } = aToB.ToFrozenDictionary();
    public FrozenDictionary<TEntity, TEntity?> BToA { get; } = bToA.ToFrozenDictionary();

    public FrozenDictionary<TEntity, TEntity> MatchedAToB { get; } = aToB
        .Where(x => x.Value is not null)
        .ToFrozenDictionary(x => x.Key, x => x.Value!);

    public FrozenDictionary<TEntity, TEntity> MatchedBToA { get; } = bToA
        .Where(x => x.Value is not null)
        .ToFrozenDictionary(x => x.Key, x => x.Value!);

    public ImmutableHashSet<TEntity> UnmatchedA { get; } =
    [
        ..aToB
            .Where(x => x.Value is null)
            .Select(x => x.Key)
    ];

    public ImmutableHashSet<TEntity> UnmatchedB { get; } =
    [
        ..aToB
            .Where(x => x.Value is null)
            .Select(x => x.Key)
    ];

    public TEntity? GetMatchFromEither(TEntity key)
    {
        return AToB.TryGetValue(key, out var value) ? value : BToA.TryGetValue(key, out value) ? value : default;
    }

    public bool HasMatch(TEntity key)
    {
        return AToB.ContainsKey(key) && BToA.ContainsKey(key);
    }
}