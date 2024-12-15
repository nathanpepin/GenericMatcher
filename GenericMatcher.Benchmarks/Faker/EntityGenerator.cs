using System.Globalization;
using Bogus;
using GenericMatcher.Benchmarks.Data;

namespace GenericMatcher.Benchmarks.Faker;

public static class EntityGenerator
{
    public static readonly Faker<TestEntity> Faker =
        new Faker<TestEntity>()
            .CustomInstantiator(f => new TestEntity(
                Id: Guid.NewGuid(),
                Name: f.Name.FullName(),
                Email: f.Internet.Email(),
                PhoneNumber: f.Phone.PhoneNumber(),
                DateOfBirth: DateOnly.FromDateTime(f.Date.Past(50)),
                Address: f.Address.FullAddress()));

    public static readonly IReadOnlyList<IMatchDefinition<TestEntity, TestMatchType>> MatchDefinitions =
    [
        new Id(),
        new Name(),
        new Email(),
        new Phone(),
        new DateOfBirth(),
        new Address()
    ];
}

public class Id : MatchDefinition<TestEntity, TestMatchType, Guid>
{
    public override TestMatchType MatchType => TestMatchType.Id;
    public override Func<TestEntity, Guid> Conversion { get; } = static x => x.Id;
}

public class Name : MatchDefinition<TestEntity, TestMatchType, string>, IMatchDefinitionString<TestEntity>
{
    public override TestMatchType MatchType => TestMatchType.Name;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Name.ToLowerInvariant();
    public Func<TestEntity, ReadOnlySpan<char>> ConvertToSpan { get; } = static x => x.Name.AsSpan();
}

public class Email : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Email;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Email.ToLowerInvariant();
}

public class Phone : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Phone;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.PhoneNumber.ToLowerInvariant();
}

public class DateOfBirth : MatchDefinition<TestEntity, TestMatchType, DateOnly>
{
    public override TestMatchType MatchType => TestMatchType.DateOfBirth;
    public override Func<TestEntity, DateOnly> Conversion { get; } = static x => x.DateOfBirth;
}

public class Address : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Address;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Address;
}