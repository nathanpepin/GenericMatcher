namespace GenericMatcher.Exceptions;

public sealed class MatchDefinitionNotSeededException()
    : EntityMatcherException("The match definition is not yet seeded");