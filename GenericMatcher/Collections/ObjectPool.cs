namespace GenericMatcher.Collections;

/// <summary>
/// ObjectPool implementation optimized for dictionary reuse
/// </summary>
internal sealed class ObjectPool<T>(int size = 32)
    where T : class, new()
{
    private readonly T?[] _items = new T[size];
    private readonly Func<T> _factory = () => new T();

    public T Get()
    {
        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item == null) continue;
            
            items[i] = null;
            return item;
        }
        
        return _factory();
    }

    public void Return(T item)
    {
        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] != null) continue;
            
            items[i] = item;
            break;
        }
    }
}