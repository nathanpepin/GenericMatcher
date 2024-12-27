using GenericMatcher.MatchDefinition;

namespace GenericMatcher.App.Data;

public sealed class Person
{
    public Person(string memberIdentificationNumber, string? ssn, bool isEmployee, DateOnly dateOfBirth)
    {
        MemberIdentificationNumber = memberIdentificationNumber;
        Ssn = ssn;
        IsEmployee = isEmployee;
        DateOfBirth = dateOfBirth;
    }

    public string MemberIdentificationNumber { get; }
    public string? Ssn { get; }
    public bool IsEmployee { get; }
    public DateOnly DateOfBirth { get; }
}

public class PersonSnnMatchDefinition : MatchDefinition<Person, PersonMatchType, string>
{
    public override PersonMatchType MatchType => PersonMatchType.Ssn;
    public override Func<Person, string> Conversion { get; } = static x => x.MemberIdentificationNumber.ToLowerInvariant();
    public Func<Person, ReadOnlySpan<char>> ConvertToSpan { get; } = static x => x.MemberIdentificationNumber;
}

public class PersonIsEmployeeMatchDefinition : MatchDefinition<Person, PersonMatchType, bool>
{
    public override PersonMatchType MatchType => PersonMatchType.IsEmployee;
    public override Func<Person, bool> Conversion { get; } = static x => x.IsEmployee;
}

public class PersonDepSnnMatchDefinition : MatchDefinition<Person, PersonMatchType, string>
{
    public override PersonMatchType MatchType => PersonMatchType.DepSsn;
    public override Func<Person, string> Conversion { get; } = static x => (x.Ssn ?? string.Empty).ToLowerInvariant();
}

public class PersonDobMatchDefinition : MatchDefinition<Person, PersonMatchType, DateOnly>
{
    public override PersonMatchType MatchType => PersonMatchType.DateOfBirth;
    public override Func<Person, DateOnly> Conversion { get; } = static x => x.DateOfBirth;
}