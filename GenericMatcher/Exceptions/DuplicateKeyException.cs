namespace GenericMatcher.Exceptions;

public sealed class DuplicateKeyException()
    : EntityMatcherException("Found two matches when only one was expected");