namespace CommonZ.Utils
{
    public class Mapping<Key, Value> : Dictionary<Key, Value>
        where Key : notnull
    {
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
