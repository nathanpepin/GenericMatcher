namespace GenericMatcher.Exceptions;

public sealed class NoMatchDefinitionException()
    : EntityMatcherException("At least one match definition is required");