using System.Collections.Immutable;

namespace GenericMatcher.EntityMatch;

public readonly struct MatchingResult<TEntity, TMatchType>
    where TEntity : class
    where TMatchType : struct, Enum
{
    public static MatchingResult<TEntity, TMatchType> Empty { get; } = new();

    public MatchingResult()
    {
        IsDuplicate = false;
        Match = null;
        Requirements = ImmutableArray<TMatchType>.Empty;
    }

    public MatchingResult(TEntity match, IEnumerable<TMatchType> matches, bool isDuplicate)
    {
        Match = match;
        IsDuplicate = isDuplicate;
        Requirements = [..matches];
    }

    public TEntity? Match { get; }
    public ImmutableArray<TMatchType> Requirements { get; }
    
    public bool IsDuplicate { get; }

    public static implicit operator bool(MatchingResult<TEntity, TMatchType> result) => result.Match is null;
    public static implicit operator TEntity?(MatchingResult<TEntity, TMatchType> result) => result.Match;
}