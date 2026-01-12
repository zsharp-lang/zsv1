namespace CommonZ.Utils
{
    public class Mapping<Key, Value> : Dictionary<Key, Value>
        where Key : notnull
    {
        public Mapping() : base() { }

        public Mapping(IDictionary<Key, Value> dictionary) : base(dictionary) { }

        public Mapping(IEnumerable<KeyValuePair<Key, Value>> items) : base(items) { }

        public virtual void OnAdd(Key key, Value value) { }

        public virtual void OnRemove(Key key) { }

        public new void Add(Key key, Value value)
        {
            OnAdd(key, value);
            base.Add(key, value);
        }

        public new void Remove(Key key)
        {
            OnRemove(key);
            base.Remove(key);
        }
    }
}
