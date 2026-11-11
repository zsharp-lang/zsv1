using System.Collections;

namespace CommonZ.Utils
{
    public readonly struct Enumerate<T> : IEnumerable<(int, T)>
    {
        private readonly IEnumerable<T> _enumerable;
        private readonly int _start;
        private readonly int _step;

        public Enumerate(IEnumerable<T> enumerable, int start = 0, int step = 1)
        {
            _enumerable = enumerable;
            _start = start;
            _step = step;
        }

        public IEnumerator<(int, T)> GetEnumerator()
        {
            int i = _start;
            foreach (var item in _enumerable)
            {
                yield return (i, item);
                i += _step;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
