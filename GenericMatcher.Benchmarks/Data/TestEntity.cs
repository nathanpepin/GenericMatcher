namespace GenericMatcher.Benchmarks.Data;

public sealed record TestEntity(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string Address);