using System.Collections;

namespace ZSharp.Compiler
{
    internal sealed partial class DependencyGraph<T>
        : IEnumerable<IEnumerable<T>>
    {
        public IEnumerator<IEnumerable<T>> GetEnumerator()
            => new Iterator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
