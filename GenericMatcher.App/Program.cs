using GenericMatcher;
using GenericMatcher.App.Data;
using GenericMatcher.EntityMatch;

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

MatchDefinition<Person, PersonMatchType>[] definitions =
[
    new(
        PersonMatchType.Ssn,
        p => p.MemberIdentificationNumber.ToUpperInvariant()),

    new(
        PersonMatchType.DepSsn,
        p => p.Ssn?.ToUpperInvariant() ?? "000000000"),

    new(
        PersonMatchType.IsEmployee,
        p => p.IsEmployee),

    new(
        PersonMatchType.DateOfBirth,
        p => p.DateOfBirth)
];


var matcher = new EntityMatcher<Person, PersonMatchType>(aPersons, definitions);

var j = matcher.FindMatches(bPersons[0], PersonMatchType.DateOfBirth);

var jj = matcher.CreateTwoWayMatchDictionary(bPersons, PersonMatchType.Ssn);

;