using System.Collections.Immutable;

namespace GenericMatcher;

public sealed record MatchResult<TEntity, TMatchType>
    where TEntity : class where TMatchType : Enum
{
    public required ImmutableArray<TEntity> Matches { get; init; }
    public required ImmutableArray<TMatchType> MatchTypes { get; init; }
    public required bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}