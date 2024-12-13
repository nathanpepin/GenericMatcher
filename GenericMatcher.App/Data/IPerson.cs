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