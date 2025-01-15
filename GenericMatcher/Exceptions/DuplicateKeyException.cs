namespace GenericMatcher.Exceptions;

public sealed class DuplicateKeyException(string entity, string entities, string criteria)
    : EntityMatcherException($"""
                              Found two matches when only one was expected:

                              Criteria: {criteria}

                              Entity: {entity}

                              Entities: {entities}
                              """);