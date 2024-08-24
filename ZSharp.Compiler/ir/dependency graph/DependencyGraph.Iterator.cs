using System.Collections;

namespace ZSharp.Compiler
{ 
    internal sealed partial class DependencyGraph<T>
    {
        internal sealed class Iterator(DependencyGraph<T> graph)
        : IEnumerator<IEnumerable<T>>
        {
            private readonly DependencyGraph<T> graph = graph;

            private int index = 0;

            public IEnumerable<T> Current => graph.order[index];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
                => ++index >= graph.order.Count;

            public void Reset()
                => index = 0;
        }
    }
}
