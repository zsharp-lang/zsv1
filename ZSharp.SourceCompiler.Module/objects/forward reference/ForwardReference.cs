using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    internal sealed class ForwardReference
        : CompilerObject
        , IProxy
    {
        public CompilerObject Inner { get; set; } = EmptyReference.Instance;

        bool CompilerObject.Is<T>([NotNullWhen(true)] out T? result) where T : class
            => Inner.Is(out result);

        Result<R> IProxy.Apply<R>(Func<CompilerObject, Result<R>> fn)
            => fn(Inner);
    }
}
