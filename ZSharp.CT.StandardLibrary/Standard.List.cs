using ZSharp.Runtime.NET.IL2IR;

namespace Standard.List
{
    public sealed class List<T>
    {
        private readonly System.Collections.Generic.List<T> _inner = [];

        public List() { }

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
    }
}
