using System.Diagnostics.CodeAnalysis;

namespace CommonZ.Utils
{
    public abstract class CacheBase<Key, Value>
        where Key : notnull
        where Value : class
    {
        private readonly Dictionary<Key, Value> _cache = new();

        public Cache<Key, Value>? Parent { get; set; }

        public T Cache<T>(Key key, T value)
            where T : Value
        {
            _cache.Add(key, value);
            return value;
        }

        public Value? Cache(Key key)
        {
            if (_cache.TryGetValue(key, out Value? result))
                return result;
            return Parent?.Cache(key);
        }

        public T? Cache<T>(Key key)
            where T : class
        {
            if (_cache.TryGetValue(key, out Value? result))
                return result as T;
            return Parent?.Cache<T>(key);
        }

        public bool Cache(Key key, [MaybeNullWhen(false)] out Value value)
        {
            if (_cache.TryGetValue(key, out value))
                return true;
            return Parent?.Cache(key, out value) ?? false;
        }

        public bool Cache<T>(Key key, [MaybeNullWhen(false)] out T? value)
            where T : class
        {
            if (_cache.TryGetValue(key, out Value? result))
                return (value = result as T) is not null;
            return Parent?.Cache(key, out value) ?? (value = null) is null;
        }

        public bool Uncache(Key key)
        {
            return _cache.Remove(key);
        }
    }
}
