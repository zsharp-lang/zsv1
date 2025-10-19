namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type)
            => Result<CastResult>.Error(
                "Cast operation is not supported."
            );
    }
}
