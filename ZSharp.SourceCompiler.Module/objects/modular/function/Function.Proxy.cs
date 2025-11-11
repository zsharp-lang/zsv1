
namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
        : IProxy
    {
        public required CompilerObject Inner { get; init; }

        IResult<R, Error> IProxy.Apply<R>(Func<CompilerObject, IResult<R, Error>> fn)
            => fn(Inner);
    }
}
