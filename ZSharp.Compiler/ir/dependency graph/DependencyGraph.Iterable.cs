using System.Collections;

namespace ZSharp.Compiler
{
    internal sealed partial class DependencyGraph<T>
        : IEnumerable<IEnumerable<DependencyNode<T>>>
    {
        public IEnumerator<IEnumerable<DependencyNode<T>>> GetEnumerator()
            => (order ?? CalculateOrder()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
