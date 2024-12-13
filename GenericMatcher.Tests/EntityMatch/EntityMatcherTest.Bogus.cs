using Bogus;
using FluentAssertions;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Tests.EntityMatch;

public sealed class EntityMatcherTestsBogus
{
    private static readonly Faker<TestEntity> EntityFaker =
        new Faker<TestEntity>()
            .CustomInstantiator(f => new TestEntity(
                Id: Guid.NewGuid(),
                Name: f.Name.FullName(),
                Email: f.Internet.Email(),
                PhoneNumber: f.Phone.PhoneNumber(),
                DateOfBirth: DateOnly.FromDateTime(f.Date.Past(50)),
                Address: f.Address.FullAddress()));

    private static readonly IReadOnlyList<MatchDefinition<TestEntity, TestMatchType>> MatchDefinitions =
    [
        new(TestMatchType.Id, entity => entity.Id),
        new(TestMatchType.Name, entity => entity.Name.ToLowerInvariant()),
        new(TestMatchType.Email, entity => entity.Email.ToLowerInvariant()),
        new(TestMatchType.Phone, entity => entity.PhoneNumber),
        new(TestMatchType.DateOfBirth, entity => entity.DateOfBirth),
        new(TestMatchType.Address, entity => entity.Address.ToLowerInvariant())
    ];

    [Fact]
    public void FindMatches_WithExactMatch_ReturnsCorrectEntity()
    {
        // Arrange
        var seedEntity = EntityFaker.Generate();
        var matchingEntity = seedEntity with { Id = Guid.NewGuid() };
        var matcher = CreateMatcher([seedEntity]);

        // Act
        var result = matcher.FindMatches(
            matchingEntity, TestMatchType.Name, TestMatchType.Email);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(seedEntity, options => options.Excluding(e => e.Id));
    }

    [Fact]
    public void FindMatches_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var seedEntities = EntityFaker.Generate(5);
        var nonMatchingEntity = EntityFaker.Generate();
        var matcher = CreateMatcher(seedEntities);

        // Act
        var result = matcher.FindMatches(
            nonMatchingEntity, TestMatchType.Email, TestMatchType.Phone);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CreateTwoWayMatches_WithPartialMatches_ReturnsCorrectMapping()
    {
        // Arrange
        var seedEntities = GenerateEntitiesWithSomeMatching(
            totalCount: 10,
            matchingCount: 5,
            out var matchingEntities);

        var matcher = CreateMatcher(seedEntities);
        var matchTypes = new[] { TestMatchType.Email, TestMatchType.Phone };

        // Act
        var result = matcher.CreateTwoWayMatchDictionary(matchingEntities, matchTypes);

        // Assert
        result.MatchedAToB.Count.Should().Be(5);
        result.UnmatchedA.Count.Should().Be(5);
        result.UnmatchedB.Count.Should().Be(5);
    }

    [Fact]
    public void CreateTieredMatches_WithMultipleTiers_MatchesCorrectly()
    {
        // Arrange
        var (seedEntities, candidateEntities) = GenerateEntitiesForTieredMatching(10);
        var matcher = CreateMatcher(seedEntities);

        var matchTiers = new[]
        {
            new[] { TestMatchType.Email, TestMatchType.Phone },
            new[] { TestMatchType.Name, TestMatchType.DateOfBirth },
            new[] { TestMatchType.Address }
        };

        // Act
        var result = matcher.CreateTwoWayMatchDictionaryTiered(candidateEntities, matchTiers);

        // Assert
        result.AToB.Count.Should().BeGreaterThan(0);
        result.UnmatchedA.Count.Should().BeLessThan(seedEntities.Count);
    }

    private static EntityMatcher<TestEntity, TestMatchType> CreateMatcher(IEnumerable<TestEntity> seedEntities) =>
        new(seedEntities, MatchDefinitions);

    private static List<TestEntity> GenerateEntitiesWithSomeMatching(
        int totalCount,
        int matchingCount,
        out IReadOnlyList<TestEntity> matchingEntities)
    {
        var seeds = EntityFaker.Generate(totalCount);
        matchingEntities = seeds
            .Take(matchingCount)
            .Select(e => e with { Id = Guid.NewGuid() })
            .ToList();

        return seeds;
    }

    private static (IReadOnlyList<TestEntity> Seeds, IReadOnlyList<TestEntity> Candidates)
        GenerateEntitiesForTieredMatching(int count)
    {
        var seeds = EntityFaker.Generate(count);

        var candidates = seeds.Select((seed, i) => i switch
        {
            _ when i < count * 0.3 =>
                seed with
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = seed.PhoneNumber
                },
            _ when i < count * 0.6 =>
                seed with
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
        }).ToList();

        return (seeds, candidates);
    }
}