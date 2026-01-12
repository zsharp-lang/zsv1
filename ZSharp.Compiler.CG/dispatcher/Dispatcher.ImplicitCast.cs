namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult ImplicitCast(CompilerObject @object, CompilerObject type)
            => Result.Error(
                "Implicit casting is not supported for the given object and type."
            );
    }
}
