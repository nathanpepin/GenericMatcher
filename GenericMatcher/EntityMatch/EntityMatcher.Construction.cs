using System.Collections.Frozen;
using System.Collections.Immutable;
using GenericMatcher.Collections;
using GenericMatcher.Exceptions;

namespace GenericMatcher.EntityMatch;

public readonly partial struct EntityMatcher<TEntity, TMatchType> where TEntity : class where TMatchType : struct, Enum
{
    public EntityMatcher(
        IEnumerable<TEntity> seedEntities,
        IEnumerable<IMatchDefinition<TEntity, TMatchType>> matchDefinitions)
    {
        ArgumentNullException.ThrowIfNull(seedEntities);
        ArgumentNullException.ThrowIfNull(matchDefinitions);

        var definitions = matchDefinitions.ToArray();
        ValidateMatchDefinitions(definitions);

        _seedEntities = seedEntities.ToFrozenSet();

        foreach (var definition in definitions)
        {
            definition.Seed(_seedEntities);
        }

        _matchStrategies = definitions
            .ToFrozenDictionary(x => x.MatchType);

        _dictionaryCache = _seedEntities
            .ToNullDictionary()
            .ToImmutableDictionary();
    }

    private static void ValidateMatchDefinitions(
        IMatchDefinition<TEntity, TMatchType>[] definitions)
    {
        if (definitions.Length == 0)
            throw new NoMatchDefinitionException();

        var seenTypes = new HashSet<TMatchType>();
        var duplicateTypes = definitions
            .Where(definition => !seenTypes.Add(definition.MatchType))
            .Select(definition => definition.MatchType.ToString())
            .ToArray();

        if (duplicateTypes.Length != 0)
            throw new DuplicateMatchTypesException([..duplicateTypes]);
    }
}