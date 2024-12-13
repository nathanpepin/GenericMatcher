namespace GenericMatcher;

/// <summary>
///     A definition to match on for the entity matcher.
///     It will group the entity via the selector and allow for lookups via the selector.
/// </summary>
/// <param name="MatchType"></param>
/// <param name="KeySelector"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TMatchType"></typeparam>
public sealed record MatchDefinition<TEntity, TMatchType>(
    TMatchType MatchType,
    Func<TEntity, object> KeySelector);