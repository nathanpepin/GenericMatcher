namespace GenericMatcher.EntityMatch;

public readonly struct ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
    public ReferenceEqualityComparer()
    {
    }

    public static ReferenceEqualityComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(T obj)
    {
        return obj?.GetHashCode() ?? 0;
    }
}