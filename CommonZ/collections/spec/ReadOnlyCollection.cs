namespace CommonZ.Utils
{
    public class ReadOnlyCollection<T> : Collection<T>
    {
        public ReadOnlyCollection() : base() { }

        public ReadOnlyCollection(IEnumerable<T> collection) : base(collection) { }

        public override void OnAdd(T item) => throw new NotSupportedException();

        public override void OnInsert(int index, T item) => throw new NotSupportedException();

        public override void OnRemove(T item) => throw new NotSupportedException();

        public override void OnRemoveAt(int index) => throw new NotSupportedException();

        public new void Add(T item) => throw new NotSupportedException();

        public new void Insert(int index, T item) => throw new NotSupportedException();

        public new bool Remove(T item) => throw new NotSupportedException();

        public new void RemoveAt(int index) => throw new NotSupportedException();
    }
}
