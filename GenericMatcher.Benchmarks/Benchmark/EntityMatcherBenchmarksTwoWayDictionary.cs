using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using GenericMatcher.Benchmarks.Data;
using GenericMatcher.Benchmarks.Faker;
using GenericMatcher.EntityMatch;

namespace GenericMatcher.Benchmarks.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
public class EntityMatcherBenchmarksTwoWayDictionary
{
    [Params(100, 1_000, 10_000, 100_000)] public int EntityCount { get; set; }

    private EntityMatcher<TestEntity, TestMatchType>? _matcher;
    private TestEntity? _testEntity;
    private List<TestEntity>? _otherEntities;

    [GlobalSetup]
    public void Setup()
    {
        var seedEntities = EntityGenerator.Faker.Generate(EntityCount);
        _matcher = new EntityMatcher<TestEntity, TestMatchType>([..seedEntities], [..EntityGenerator.MatchDefinitions]);
        _testEntity = EntityGenerator.Faker.Generate();
        _otherEntities = EntityGenerator.Faker.Generate(EntityCount / 2);
    }

    [Benchmark(Description = "Two-Way Dictionary")]
    public void TwoWayDictionary()
    {
        _matcher!.Value.CreateTwoWayMatchDictionary([.._otherEntities!], [TestMatchType.Id]);
    }


    // [Benchmark(Description = "Two-Way Dictionary Tiered")]
    // public void TwoWayDictionaryTiere()
    // {
    //     var tiers = new[]
    //     {
    //         new[] { TestMatchType.Email, TestMatchType.Phone },
    //         new[] { TestMatchType.Name, TestMatchType.DateOfBirth },
    //         new[] { TestMatchType.Address }
    //     };
    //
    //     _matcher!.CreateStrictTwoWayMatchDictionary(_otherEntities!, tiers);
    // }
}