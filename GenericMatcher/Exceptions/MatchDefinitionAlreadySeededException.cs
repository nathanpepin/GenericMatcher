namespace GenericMatcher.Exceptions;

public sealed class MatchDefinitionAlreadySeededException()
    : EntityMatcherException("The match definition has already been seeded");