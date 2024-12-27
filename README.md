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

Sample benchmark results for two-way dictionary matching:

| Entity Count | Mean Time | Allocated Memory |
|-------------|-----------|------------------|
| 100         | 44.25 μs  | 60.63 KB        |
| 1,000       | 495.97 μs | 540.53 KB       |
| 10,000      | 5.73 ms   | 5.02 MB         |
| 100,000     | 83.66 ms  | 47.76 MB        |

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