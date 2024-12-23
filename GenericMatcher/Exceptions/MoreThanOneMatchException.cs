namespace GenericMatcher.Exceptions;

public sealed class MoreThanOneMatchException()
    : EntityMatcherException("Found two matches when only one was expected");