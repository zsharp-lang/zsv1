namespace CommonZ.Utils;

public class Collection<T> : List<T> {
    public static readonly Collection<T> Empty = new ReadOnlyCollection<T>();

    public Collection() : base() { }

    public Collection(IEnumerable<T> collection) : base(collection) { }

    public virtual void OnAdd(T item) { }

    public virtual void OnInsert(int index, T item) { }

    public virtual void OnRemove(T item) { }

    public virtual void OnRemoveAt(int index) { }

    public new void Add(T item) {
        OnAdd(item);
        base.Add(item);
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
            Add(item);
    }

    public new void Insert(int index, T item) {
        OnInsert(index, item);
        base.Insert(index, item);
    }

    public new bool Remove(T item) {
        OnRemove(item);
        return base.Remove(item);
    }

    public new void RemoveAt(int index) {
        OnRemoveAt(index);
        base.RemoveAt(index);
    }
}
