namespace GenericMatcher.Exceptions;

public sealed class DuplicateMatchTypesException(IEnumerable<string> duplicateTypes)
    : EntityMatcherException($"Duplicate match types found: {string.Join(", ", duplicateTypes)}");