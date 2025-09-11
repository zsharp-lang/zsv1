using ZSharp.Runtime.NET.IL2IR;

namespace Standard.List
{
    public sealed class ListIterator<T>
    {
        internal IEnumerator<T> Enumerator { get; init; }

        [Alias(Name = "moveNext")]
        public bool MoveNext()
            => Enumerator.MoveNext();

        [Alias(Name = "getCurrent")]
        public T GetCurrentValue()
            => Enumerator.Current;
    }
}
