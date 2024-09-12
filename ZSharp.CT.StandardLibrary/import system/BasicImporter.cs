using ZSharp.Compiler;

namespace ZSharp.CT.StandardLibrary
{
    public abstract class BasicImporter<T>
    {
        internal protected abstract CGObject Import(T source, Argument[] arguments);
    }
}
