
namespace ZSharp.SourceCompiler.Objects
{
    public sealed class ForwardReference(CompilerObject type)
        : CompilerObject
        , IProxy
    {
        public CompilerObject? Value { get; set; }

        public CompilerObject Type { get; } = type;

        IResult<R, Error> IProxy.Apply<R>(Func<CompilerObject, IResult<R, Error>> fn)
            => Value is not null
                ? fn(Value)
                : Result<R>.Error(
                    new ErrorMessage(
                        $"Forward reference of type {Type} has not been resolved yet."
                    )
                );
    }
}
