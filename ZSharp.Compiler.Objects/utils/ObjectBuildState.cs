namespace ZSharp.Objects
{
    internal sealed class ObjectBuildState<T>
        where T : struct, Enum, IConvertible
    {
        public T State { get; private set; }

        public bool Get(T state) => State.HasFlag(state);

        public void Set(T state)
        {
            State = (T)(ValueType)(State.ToInt32(null) | state.ToInt32(null));
        }
    }
}
