namespace CommonZ.Utils
{
    public class Cache<Key, Value> : CacheBase<Key, Value>
        where Key : notnull
        where Value : class
    {
    }

    public class Cache<Key> : CacheBase<Key, object>
        where Key : notnull
    {
    }

    public class Cache : CacheBase<object, object>
    {
    }
}
