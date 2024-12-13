using System.Collections.Immutable;
using Bogus;
using FluentAssertions;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Tests.EntityMatch;

public sealed class EntityMatcherTests
{
    // Extract common GUIDs and DateOnly values as constants
    private static readonly Guid Guid1 = Guid.Parse("7f9e1a2b-3c4d-5e6f-7a8b-9c0d1e2f3a4b");
    private static readonly Guid Guid2 = Guid.Parse("2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c7d");
    private static readonly Guid Guid3 = Guid.Parse("3b4c5d6e-7f8a-9b0c-1d2e-3f4a5b6c7d8e");
    private static readonly Guid Guid4 = Guid.Parse("4c5d6e7f-8a9b-0c1d-2e3f-4a5b6c7d8e9f");
    private static readonly Guid Guid5 = Guid.Parse("5d6e7f8a-9b0c-1d2e-3f4a-5b6c7d8e9f0a");
    private static readonly DateOnly Dob1 = new(1985, 3, 15);
    private static readonly DateOnly Dob2 = new(1988, 11, 30);
    private static readonly DateOnly Dob3 = new(1992, 5, 8);
    private static readonly DateOnly Dob4 = new(1987, 9, 4);

    // Extract test entities and definitions into separate properties
    private static readonly ImmutableArray<TestEntity> MockedEntities =
    [
        CreateTestEntity(Guid1, "Alice Chen", "alice.chen@example.com", "+1 (555) 123-4567", Dob1, "123 Maple Street, Springfield, IL 62701"),
        CreateTestEntity(Guid2, "Marcus Rodriguez", "m.rodriguez@example.com", "+1 (555) 234-5678", Dob1, "456 Oak Avenue, Riverside, CA 92501"),
        CreateTestEntity(Guid3, "Sarah Thompson", "sarah.t@example.com", "+1 (555) 345-6789", Dob2, "789 Pine Road, Austin, TX 78701"),
        CreateTestEntity(Guid4, "David Kim", "david.kim@example.com", "+1 (555) 456-7890", Dob3, "321 Elm Court, Seattle, WA 98101"),
        CreateTestEntity(Guid5, "Emma Wilson", "e.wilson@example.com", "+1 (555) 567-8901", Dob4, "654 Birch Lane, Boston, MA 02108")
    ];

    private static readonly IReadOnlyList<MatchDefinition<TestEntity, TestMatchType>> MatchDefinitions = new List<MatchDefinition<TestEntity, TestMatchType>>
    {
        new(TestMatchType.Id, entity => entity.Id),
        new(TestMatchType.Name, entity => entity.Name.ToLowerInvariant()),
        new(TestMatchType.Email, entity => entity.Email.ToLowerInvariant()),
        new(TestMatchType.Phone, entity => entity.PhoneNumber),
        new(TestMatchType.DateOfBirth, entity => entity.DateOfBirth),
        new(TestMatchType.Address, entity => entity.Address.ToLowerInvariant())
    };

    [Fact]
    public void Should_Find_Entity_By_Id()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var testEntity = MockedEntities[1];

        // Act
        var result = matcher.FindMatches(testEntity, TestMatchType.Id);

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
        var idDobResult = matcher.FindMatches(testEntity, TestMatchType.Id, TestMatchType.DateOfBirth);
        var dobResult = matcher.FindMatches(testEntity, TestMatchType.DateOfBirth);

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
        var twoWayDictionary = matcher.CreateTwoWayMatchDictionary(mutatedEntities, TestMatchType.Name, TestMatchType.DateOfBirth, TestMatchType.Phone);

        // Assert
        twoWayDictionary.AToB.Count.Should().Be(5);
        twoWayDictionary.BToA.Count.Should().Be(5);
        twoWayDictionary.MatchedAToB.Count.Should().Be(5);
        twoWayDictionary.MatchedBToA.Count.Should().Be(5);
    }

    [Fact]
    public void Should_Create_TwoWay_Dictionary_With_Partial_Matches()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var partialMatches = MockedEntities.Take(2).Select(x => x with { Id = Guid.NewGuid() }).ToImmutableArray();

        // Act
        var twoWayDictionary = matcher.CreateTwoWayMatchDictionary(partialMatches, TestMatchType.Name, TestMatchType.DateOfBirth, TestMatchType.Phone);

        // Assert
        twoWayDictionary.AToB.Count.Should().Be(5);
        twoWayDictionary.BToA.Count.Should().Be(2);
        twoWayDictionary.MatchedAToB.Count.Should().Be(2);
        twoWayDictionary.MatchedBToA.Count.Should().Be(2);
    }

    [Fact]
    public void Should_Handle_Multiple_Match_Criteria_By_Id()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var specificMatches = MockedEntities
            .Select((x, i) => x with { Id = i is 0 or 1 ? x.Id : Guid.NewGuid() })
            .ToImmutableArray();

        // Act
        var twoWayDictionary = matcher.CreateTwoWayMatchDictionary(specificMatches, TestMatchType.Id);

        // Assert
        twoWayDictionary.AToB.Count.Should().Be(5);
        twoWayDictionary.BToA.Count.Should().Be(5);
        twoWayDictionary.MatchedAToB.Count.Should().Be(2);
        twoWayDictionary.MatchedBToA.Count.Should().Be(2);
    }

    [Fact]
    public void Should_Have_Proper_TwoWay_Dictionary_Returns()
    {
        // Arrange
        var matcher = CreateEntityMatcher();
        var specificMatches = MockedEntities
            .Select((x, i) => x with { Id = i is 0 or 1 ? x.Id : Guid.NewGuid() })
            .ToImmutableArray();
        var outsideCollection = new TestEntity(Guid.NewGuid(), string.Empty, string.Empty, string.Empty, DateOnly.MinValue, string.Empty);

        // Act
        var twoWayDictionary = matcher.CreateTwoWayMatchDictionary(specificMatches, TestMatchType.Id);

        // Assert
        twoWayDictionary.AToB.Count.Should().Be(5);
        twoWayDictionary.BToA.Count.Should().Be(5);
        twoWayDictionary.MatchedAToB.Count.Should().Be(2);
        twoWayDictionary.MatchedBToA.Count.Should().Be(2);
        twoWayDictionary.UnmatchedA.Should().NotContain(outsideCollection);
        twoWayDictionary.UnmatchedB.Should().NotContain(outsideCollection);
        twoWayDictionary.UnmatchedA.Should().NotContain(specificMatches[0]);
        twoWayDictionary.UnmatchedB.Should().NotContain(specificMatches[0]);
        twoWayDictionary.HasMatch(specificMatches[0]).Should().BeTrue();
        twoWayDictionary.HasMatch(outsideCollection).Should().BeFalse();
        twoWayDictionary.GetMatchFromEither(MockedEntities[0]).Should().BeEquivalentTo(specificMatches[0]);
        twoWayDictionary.GetMatchFromEither(outsideCollection).Should().BeNull();
    }

    // Helper Methods: Factory Pattern for Common Instantiations
    private static TestEntity CreateTestEntity(Guid id, string name, string email, string phoneNumber, DateOnly dob, string address) =>
        new(id, name, email, phoneNumber, dob, address);

    private static EntityMatcher<TestEntity, TestMatchType> CreateEntityMatcher() =>
        new(MockedEntities, MatchDefinitions);
}