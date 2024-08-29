using System.Diagnostics.CodeAnalysis;

namespace CommonZ.Utils
{
    public abstract class CacheBase<Key, Value>
        where Key : notnull
        where Value : class
    {
        private readonly Dictionary<Key, Value> _cache = [];

        public Cache<Key, Value>? Parent { get; set; }

        public T Cache<T>(Key key, T value)
            where T : Value
        {
            _cache.Add(key, value);
            return value;
        }

        public Value? Cache(Key key, bool searchParent = true)
        {
            if (_cache.TryGetValue(key, out Value? result))
                return result;
            if (!searchParent) return null;
            return Parent?.Cache(key);
        }

        public T? Cache<T>(Key key, bool searchParent = true)
            where T : class
        {
            if (_cache.TryGetValue(key, out Value? result))
                return result as T;
            if (!searchParent) return null;
            return Parent?.Cache<T>(key);
        }

        public bool Cache(Key key, [MaybeNullWhen(false)] out Value value, bool searchParent = true)
        {
            if (_cache.TryGetValue(key, out value))
                return true;
            if (!searchParent) return (value = null) is not null;
            return Parent?.Cache(key, out value) ?? false;
        }

        public bool Cache<T>(Key key, [MaybeNullWhen(false)] out T? value, bool searchParent = true)
            where T : class
        {
            if (_cache.TryGetValue(key, out Value? result))
                return (value = result as T) is not null;
            if (!searchParent) return (value = null) is not null;
            return Parent?.Cache(key, out value) ?? (value = null) is not null;
        }

        public bool Uncache(Key key)
        {
            return _cache.Remove(key);
        }
    }
}
