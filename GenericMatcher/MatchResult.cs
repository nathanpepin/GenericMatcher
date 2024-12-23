using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher;

public readonly struct MatchResult<TEntity, TMatchType>(ReadOnlySpan<TEntity> matches, TMatchType[] requirements)
    where TEntity : class
    where TMatchType : struct, Enum
{
    public TEntity[] Matches { get; } = matches.ToArray();
    public TMatchType[] MatchRequirementsMet { get; } = requirements;
    public bool FoundMatches => Matches.Length == 0;

    public static MatchResult<TEntity, TMatchType> Empty => new([], []);
}