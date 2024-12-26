// EntityMatcherTests.cs

using System.Collections.Immutable;
using FluentAssertions;
using GenericMatcher.EntityMatch;
using GenericMatcher.MatchDefinition;

namespace GenericMatcher.UnitTests.EntityMatch;

public sealed class EntityMatcherTests
{
    private static readonly Guid Guid1 = Guid.Parse("7f9e1a2b-3c4d-5e6f-7a8b-9c0d1e2f3a4b");
    private static readonly Guid Guid2 = Guid.Parse("2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c7d");
    private static readonly Guid Guid3 = Guid.Parse("3b4c5d6e-7f8a-9b0c-1d2e-3f4a5b6c7d8e");
    private static readonly Guid Guid4 = Guid.Parse("4c5d6e7f-8a9b-0c1d-2e3f-4a5b6c7d8e9f");
    private static readonly Guid Guid5 = Guid.Parse("5d6e7f8a-9b0c-1d2e-3f4a-5b6c7d8e9f0a");
    private static readonly DateOnly Dob1 = new(1985, 3, 15);
    private static readonly DateOnly Dob2 = new(1988, 11, 30);
    private static readonly DateOnly Dob3 = new(1992, 5, 8);
    private static readonly DateOnly Dob4 = new(1987, 9, 4);

    private static readonly ImmutableArray<TestEntity> MockedEntities =
    [
        CreateTestEntity(Guid1, "Alice Chen", "alice.chen@example.com", "+1 (555) 123-4567", Dob1, "123 Maple Street, Springfield, IL 62701"),
        CreateTestEntity(Guid2, "Marcus Rodriguez", "m.rodriguez@example.com", "+1 (555) 234-5678", Dob1, "456 Oak Avenue, Riverside, CA 92501"),
        CreateTestEntity(Guid3, "Sarah Thompson", "sarah.t@example.com", "+1 (555) 345-6789", Dob2, "789 Pine Road, Austin, TX 78701"),
        CreateTestEntity(Guid4, "David Kim", "david.kim@example.com", "+1 (555) 456-7890", Dob3, "321 Elm Court, Seattle, WA 98101"),
        CreateTestEntity(Guid5, "Emma Wilson", "e.wilson@example.com", "+1 (555) 567-8901", Dob4, "654 Birch Lane, Boston, MA 02108")
    ];

    private static readonly IReadOnlyList<IMatchDefinition<TestEntity, TestMatchType>> MatchDefinitions =
    [
        new IdMatch(),
        new NameMatch(),
        new EmailMatch(),
        new PhoneMatch(),
        new DateOfBirthMatch(),
        new AddressMatch()
    ];

    [Fact]
    public void Should_Find_Entity_By_Id()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var testEntity = MockedEntities[1];

        // Act
        var result = matcher.FindMatches(testEntity, TestMatchType.Id).ToArray();

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(testEntity);
    }

    [Fact]
    public void Should_Find_Entity_By_Id_And_DateOfBirth()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var testEntity = MockedEntities[1];

        // Act
        var idDobResult = matcher.FindMatches(testEntity, TestMatchType.Id, TestMatchType.DateOfBirth).ToArray();
        var dobResult = matcher.FindMatches(testEntity, TestMatchType.DateOfBirth).ToArray();

        // Assert
        idDobResult.Should().ContainSingle().Which.Should().BeEquivalentTo(testEntity);
        dobResult.Should().HaveCount(2); // Two entities match the same DateOfBirth
    }

    [Fact]
    public void Should_Create_TwoWay_Dictionary_With_Mutated_Id()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var mutatedEntities = MockedEntities.Select(x => x with { Id = Guid.NewGuid() }).ToImmutableArray();

        // Act
        var twoWayDictionary = matcher.CreateTwoWayMatchDictionary(mutatedEntities, [TestMatchType.Name, TestMatchType.DateOfBirth, TestMatchType.Phone]);

        // Assert
        twoWayDictionary.AToB.Count.Should().Be(5);
        twoWayDictionary.BToA.Count.Should().Be(5);
    }

    private static TestEntity CreateTestEntity(Guid id, string name, string email, string phoneNumber, DateOnly dob, string address) =>
        new(id, name, email, phoneNumber, dob, address);

    private static EntityMatcher<TestEntity, TestMatchType> CreateEntityMatcher() =>
        new(MockedEntities, MatchDefinitions);
}

// Match Definition Classes
public sealed class IdMatch : MatchDefinition<TestEntity, TestMatchType, Guid>
{
    public override TestMatchType MatchType => TestMatchType.Id;
    public override Func<TestEntity, Guid> Conversion { get; } = static x => x.Id;
}

public sealed class NameMatch : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Name;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Name.ToLowerInvariant();
}

public sealed class EmailMatch : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Email;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Email.ToLowerInvariant();
}

public sealed class PhoneMatch : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Phone;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.PhoneNumber;
}

public sealed class DateOfBirthMatch : MatchDefinition<TestEntity, TestMatchType, DateOnly>
{
    public override TestMatchType MatchType => TestMatchType.DateOfBirth;
    public override Func<TestEntity, DateOnly> Conversion { get; } = static x => x.DateOfBirth;
}

public sealed class AddressMatch : MatchDefinition<TestEntity, TestMatchType, string>
{
    public override TestMatchType MatchType => TestMatchType.Address;
    public override Func<TestEntity, string> Conversion { get; } = static x => x.Address.ToLowerInvariant();
}