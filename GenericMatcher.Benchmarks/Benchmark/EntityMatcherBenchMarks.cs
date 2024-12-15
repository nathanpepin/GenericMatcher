using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using GenericMatcher.Benchmarks.Data;
using GenericMatcher.Benchmarks.Faker;
using GenericMatcher.EntityMatch;
using Perfolizer.Horology;

namespace GenericMatcher.Benchmarks.Benchmark;

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(Job.ShortRun // Switch from MediumRun to ShortRun
            .WithWarmupCount(2) // Reduce warmup iterations
            .WithIterationCount(3) // Reduce measurement iterations
            .WithToolchain(InProcessNoEmitToolchain.Instance)
            .WithLaunchCount(1)); // Single launch is sufficient for most cases
        
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}

// [Config(typeof(AntiVirusFriendlyConfig))]
// [MemoryDiagnoser]
// public class EntityMatcherBenchmarks
// {
//     [Params(100, 1_000, 10_000)] public int EntityCount { get; set; }
//
//     private EntityMatcher<TestEntity, TestMatchType>? _matcher;
//     private TestEntity? _testEntity;
//     private List<TestEntity>? _otherEntities;
//
//     [GlobalSetup]
//     public void Setup()
//     {
//         var seedEntities = EntityGenerator.Faker.Generate(EntityCount);
//         _matcher = new EntityMatcher<TestEntity, TestMatchType>(seedEntities, EntityGenerator.MatchDefinitions);
//         _testEntity = EntityGenerator.Faker.Generate();
//         _otherEntities = EntityGenerator.Faker.Generate(EntityCount / 2);
//     }
//
//     [Benchmark(Description = "Constructor")]
//     public EntityMatcher<TestEntity, TestMatchType> Constructor()
//     {
//         var seedEntities = EntityGenerator.Faker.Generate(EntityCount);
//         return new EntityMatcher<TestEntity, TestMatchType>(seedEntities, EntityGenerator.MatchDefinitions);
//     }
//
//     [Benchmark(Description = "Single Match Type")]
//     public void SingleMatchType()
//     {
//         _matcher!.FindMatches(_testEntity!, TestMatchType.Email);
//     }
//
//     [Benchmark(Description = "Multiple Match Types")]
//     public void MultipleMatchTypes()
//     {
//         _matcher!.FindMatches(_testEntity!, TestMatchType.Email, TestMatchType.Phone, TestMatchType.Name);
//     }
//
//     [Benchmark(Description = "Tiered Matching")]
//     public void TieredMatching()
//     {
//         var tiers = new[]
//         {
//             new[] { TestMatchType.Email, TestMatchType.Phone },
//             new[] { TestMatchType.Name, TestMatchType.DateOfBirth },
//             new[] { TestMatchType.Address }
//         };
//
//         _matcher!.FindMatchesTiered(_testEntity!, tiers);
//     }
// }