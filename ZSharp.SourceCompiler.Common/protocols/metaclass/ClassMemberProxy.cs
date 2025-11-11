namespace ZSharp.SourceCompiler.Objects
{
    public sealed class ClassMemberProxy
        : CompilerObject
        , IProxy
    {
        public CompilerObject? Definition { get; set; }

        IResult<R, Error> IProxy.Apply<R>(Func<CompilerObject, IResult<R, Error>> fn)
        {
            if (Definition is null)
                return Result<R>.Error("Proxy definition is not set.");

            return fn(Definition);
        }
    }
}
