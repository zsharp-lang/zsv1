namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result<CastResult> Cast(CompilerObject @object, CompilerObject type)
            => Result<CastResult>.Error(
                "Cast operation is not supported."
            );
    }
}
