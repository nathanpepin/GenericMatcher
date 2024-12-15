using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GenericMatcher;

public sealed record MatchResult<TEntity, TMatchType>
    where TEntity : class where TMatchType : Enum
{
    public required FrozenSet<TEntity> Matches { get; init; }

    /// <summary>
    /// The match requirements that were met during the tiered match
    /// </summary>
    public required TMatchType[] MatchRequirementMet { get; init; }

    public bool FoundMatches => Matches.Count > 0;

    public static MatchResult<TEntity, TMatchType> Empty { get; } = new()
    {
        Matches = FrozenSet<TEntity>.Empty,
        MatchRequirementMet = []
    };
}