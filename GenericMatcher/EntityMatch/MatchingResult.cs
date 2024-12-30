using System.Collections.Immutable;

namespace GenericMatcher.EntityMatch;

public readonly struct MatchingResult<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    public static MatchingResult<TEntity, TMatchType> Empty { get; } = new();

    public MatchingResult()
    {
        Match = null;
        Requirements = ImmutableArray<TMatchType>.Empty;
    }

    public MatchingResult(TEntity match, IEnumerable<TMatchType> matches)
    {
        Match = match;
        Requirements = [..matches];
    }

    public TEntity? Match { get; }
    public ImmutableArray<TMatchType> Requirements { get; }

    public static implicit operator bool(MatchingResult<TEntity, TMatchType> result)
    {
        return result.Match is null;
    }

    public static implicit operator TEntity?(MatchingResult<TEntity, TMatchType> result)
    {
        return result.Match;
    }
}