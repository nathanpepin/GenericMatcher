using System.Collections.Frozen;
using Bogus;
using FluentAssertions;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.UnitTests.EntityMatch;

public sealed class EntityMatcherTestsBogus
{
    private static readonly Faker<TestEntity> EntityFaker =
        new Faker<TestEntity>()
            .CustomInstantiator(f => new TestEntity(
                Id: Guid.NewGuid(),
                Name: f.Name.FullName(),
                Email: f.Internet.Email().ToLowerInvariant(),
                PhoneNumber: f.Phone.PhoneNumber(),
                DateOfBirth: DateOnly.FromDateTime(f.Date.Past(50)),
                Address: f.Address.FullAddress()));

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
    public void FindMatches_WithExactMatch_ReturnsCorrectEntity()
    {
        // Arrange
        var seedEntity = EntityFaker.Generate();
        var matchingEntity = seedEntity with { Id = Guid.NewGuid() };
        var matcher = CreateMatcher([seedEntity]);

        // Act
        var result = matcher.FindMatches(matchingEntity, TestMatchType.Name, TestMatchType.Email);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(seedEntity, options => options.Excluding(e => e.Id));
    }

    [Fact]
    public void FindMatches_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var seedEntities = GenerateEntities(5);
        var nonMatchingEntity = EntityFaker.Generate();
        var matcher = CreateMatcher(seedEntities);

        // Act
        var result = matcher.FindMatches(nonMatchingEntity, TestMatchType.Email, TestMatchType.Phone);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CreateTwoWayMatches_WithPartialMatches_ReturnsCorrectMapping()
    {
        // Arrange
        var (seedEntities, matchingEntities) = GenerateMatchingSets(
            totalCount: 10,
            matchingCount: 5);

        var matcher = CreateMatcher(seedEntities);
        var matchTypes = new[] { TestMatchType.Email, TestMatchType.Phone };

        // Act
        var result = matcher.CreateTwoWayMatchDictionary(matchingEntities, matchTypes);

        // Assert
        result.MatchedAToB.Count.Should().Be(5);
        result.UnmatchedA.Count.Should().Be(5);
        result.UnmatchedB.Count.Should().Be(0);
    }

    [Fact]
    public void CreateTieredMatches_WithMultipleTiers_MatchesCorrectly()
    {
        // Arrange
        var (seedEntities, candidateEntities) = GenerateTieredMatchingData(10);
        var matcher = CreateMatcher(seedEntities);

        var matchTiers = new[]
        {
            new[] { TestMatchType.Email, TestMatchType.Phone },
            new[] { TestMatchType.Name, TestMatchType.DateOfBirth },
            new[] { TestMatchType.Address }
        };

        // Act
        var result = matcher.CreateTwoWayMatchDictionary(candidateEntities, matchTiers);

        // Assert
        result.AToB.Count.Should().BeGreaterThan(0);
        result.UnmatchedA.Count.Should().BeLessThan(seedEntities.Count);
    }

    // Helper Methods
    private static EntityMatcher<TestEntity, TestMatchType> CreateMatcher(IReadOnlyList<TestEntity> seedEntities) =>
        new(seedEntities, MatchDefinitions);

    private static IReadOnlyList<TestEntity> GenerateEntities(int count) =>
        EntityFaker.Generate(count).ToFrozenSet().ToArray();

    private static (IReadOnlyList<TestEntity> Seeds, IReadOnlyList<TestEntity> Matches) GenerateMatchingSets(
        int totalCount,
        int matchingCount)
    {
        var seeds = GenerateEntities(totalCount);
        var matches = seeds
            .Take(matchingCount)
            .Select(e => e with { Id = Guid.NewGuid() })
            .ToArray();

        return (seeds, matches);
    }

    private static (IReadOnlyList<TestEntity> Seeds, IReadOnlyList<TestEntity> Candidates)
        GenerateTieredMatchingData(int count)
    {
        var seeds = GenerateEntities(count);

        var candidates = seeds.Select((seed, i) =>
        {
            var matchPercentage = i / (double)count;
            return matchPercentage switch
            {
                < 0.3 => seed with
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = seed.PhoneNumber
                },
                < 0.6 => seed with
                {
                    Id = Guid.NewGuid(),
                    Name = seed.Name,
                    DateOfBirth = seed.DateOfBirth
                },
                _ => seed with
                {
                    Id = Guid.NewGuid(),
                    Address = seed.Address
                }
            };
        }).ToArray();

        return (seeds, candidates);
    }
}