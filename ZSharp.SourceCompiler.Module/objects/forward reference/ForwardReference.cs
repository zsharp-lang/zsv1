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

        IResult<R, Error> IProxy.Apply<R>(Func<CompilerObject, IResult<R, Error>> fn)
            => fn(Inner);
    }
}
