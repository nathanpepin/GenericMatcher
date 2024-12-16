namespace GenericMatcher.EntityMatch;

public readonly struct ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
    public ReferenceEqualityComparer()
    {
    }

    public static ReferenceEqualityComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
    public int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;
}