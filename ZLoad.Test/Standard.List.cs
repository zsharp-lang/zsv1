using ZSharp.Runtime.NET.IL2IR;

namespace Standard.List
{
    public sealed class List<T>
    {
        private readonly System.Collections.Generic.List<T> _inner = [];

        public List() { }

        [Alias(Name = "length")]
        public int Length()
            => _inner.Count;


        [Alias(Name = "append")]
        public void Add(T item)
        {
            _inner.Add(item);
        }

        [Alias(Name = "get")]
        public T Get(int index)
        {
            return _inner[index];
        }

        [Alias(Name = "removeAt")]
        public void RemoveAt(int index)
            => _inner.RemoveAt(index);

        [Alias(Name = "getIterator")]
        public ListIterator<T> GetIterator()
            => new()
            {
                Enumerator = _inner.GetEnumerator()
            };
    }
}
