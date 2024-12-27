# GenericMatcher

A high-performance .NET library for configurable entity matching and relationship mapping.

## Features

- Generic type support for flexible entity matching
- Multiple matching strategies with customizable criteria
- Two-way relationship mapping
- Tiered matching for complex matching scenarios
- Support for strict and non-strict matching modes
- High-performance frozen collections for optimal memory usage
- Thread-safe operations
- Fluent API design

## Installation

Install via NuGet:

```bash
dotnet add package GenericMatcher
```

## Quick Start

Here's a basic example of matching entities:

```csharp
// Define your entity
public record Person(string Id, string Name, string Email);

// Create match definitions
public class EmailMatch : MatchDefinition<Person, MatchType, string>
{
    public override MatchType MatchType => MatchType.Email;
    public override Func<Person, string> Conversion => x => x.Email.ToLowerInvariant();
}

// Initialize matcher
var matcher = new EntityMatcher<Person, MatchType>(
    seedEntities,
    new[] { new EmailMatch() }
);

// Find matches
var matches = matcher.FindMatches(person, MatchType.Email);
```

## Advanced Usage

### Two-Way Dictionary Matching

```csharp
// Create a two-way relationship map
var twoWayMap = matcher.CreateTwoWayMatchDictionary(
    otherEntities,
    MatchType.Email,
    MatchType.Name
);

// Access matched and unmatched results
var matchedFromA = twoWayMap.AToBMatchedResults.Value;
var unmatchedFromB = twoWayMap.BToAUnmatchedResults.Value;
```

### Tiered Matching

```csharp
var tieredResult = matcher.CreateTwoWayMatchDictionary(
    otherEntities,
    new[] { 
        new[] { MatchType.Email },
        new[] { MatchType.Name, MatchType.DateOfBirth }
    }
);
```

### Strict Matching

```csharp
// Throws exception if multiple matches are found
var strictMatches = matcher.CreateStrictTwoWayMatchDictionary(
    otherEntities,
    MatchType.Email
);
```

## Performance

GenericMatcher is designed for high performance:

- Uses frozen collections for immutable operations
- Optimized dictionary lookups
- Memory-efficient data structures
- Thread-safe operations
- Benchmarked using BenchmarkDotNet

Benchmark results for two-way dictionary matching:

| Entity Count | Mean Time | Error | StdDev | Gen0 | Gen1 | Gen2 | Allocated Memory |
|-------------|-----------|-------|---------|------|------|------|-----------------|
| 100 | 12.95 µs | ±1.633 µs | 0.090 µs | 0.4730 | - | - | 8.91 KB |
| 1,000 | 156.83 µs | ±15.867 µs | 0.870 µs | 4.3945 | 0.7324 | - | 84.14 KB |
| 10,000 | 1,630.75 µs | ±73.539 µs | 4.031 µs | 48.8281 | 39.0625 | 39.0625 | 577.64 KB |
| 100,000 | 19,729.08 µs | ±8,605.220 µs | 471.681 µs | 62.5000 | 62.5000 | 62.5000 | 5,548.95 KB |

## API Documentation

### EntityMatcher<TEntity, TMatchType>

Core class for entity matching operations.

#### Methods

- `FindMatches(TEntity entity, params TMatchType[] matchTypes)`
- `FindFirstMatchOrDefault(TEntity entity, params TMatchType[] matchTypes)`
- `CreateTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, params TMatchType[] matchTypes)`
- `CreateStrictTwoWayMatchDictionary(IEnumerable<TEntity> otherEntities, params TMatchType[] matchTypes)`

### MatchDefinition<TEntity, TMatchType, TProperty>

Base class for defining match criteria.

#### Properties

- `MatchType`: The type of match this definition represents
- `Conversion`: Function to convert entity to comparable property
- `IsSeeded`: Indicates if the definition has been initialized with seed data

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.