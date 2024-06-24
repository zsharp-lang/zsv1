using CommonZ.Utils;

namespace ZSharp.CTRuntime
{
    public interface IScope<T>
    {
        public Mapping<string, IBinding<T>> Members { get; }
    }
}
