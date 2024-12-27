using GenericMatcher.App.Data;
using GenericMatcher.EntityMatch;
using GenericMatcher.MatchDefinition;

List<Person> aPersons =
[
    new("0", "0", true, new DateOnly(1980, 1, 1)),
    new("1", "1", true, new DateOnly(1981, 1, 1)),
    new("2", "2", true, new DateOnly(1982, 1, 1)),
    new("3", "3", true, new DateOnly(1983, 1, 1)),
    new("4", "4", true, new DateOnly(1984, 1, 1)),
    new("5", "5", true, new DateOnly(1980, 1, 1)),
    new("6", "6", true, new DateOnly(1980, 1, 1)),
    new("7", "7", true, new DateOnly(1980, 1, 1))
];

List<Person> bPersons =
[
    new("0", "0", true, new DateOnly(2080, 1, 1)),
    new("1", "1", true, new DateOnly(2081, 1, 1)),
    new("2", "2", true, new DateOnly(2082, 1, 1)),
    new("3", "3", true, new DateOnly(2083, 1, 1)),
    new("4", "4", true, new DateOnly(2084, 1, 1)),
    new("5", "5", true, new DateOnly(2080, 1, 1)),
    new("6", "6", true, new DateOnly(2080, 1, 1)),
    new("7", "7", true, new DateOnly(2080, 1, 1))
];

IMatchDefinition<Person, PersonMatchType>[] definitions =
[
    new PersonSnnMatchDefinition(),
    new PersonIsEmployeeMatchDefinition(),
    new PersonDepSnnMatchDefinition(),
    new PersonDobMatchDefinition()
];


var matcher = new EntityMatcher<Person, PersonMatchType>(aPersons, definitions);

var singleLookup = matcher.FindMatches(aPersons[0], [PersonMatchType.Ssn]);
var singleMatch = matcher.FindFirstMatchOrDefault(aPersons[0], [PersonMatchType.Ssn]);
var twoWayDictionary = matcher.CreateTwoWayMatchDictionary([..bPersons], PersonMatchType.Ssn);
var twoWayStrictDictionary = matcher.CreateStrictTwoWayMatchDictionary([..bPersons], PersonMatchType.DateOfBirth, PersonMatchType.Ssn);
var twoWayTieredDictionary = matcher.CreateTwoWayMatchDictionary([..bPersons], [PersonMatchType.DateOfBirth], [PersonMatchType.Ssn]);

;