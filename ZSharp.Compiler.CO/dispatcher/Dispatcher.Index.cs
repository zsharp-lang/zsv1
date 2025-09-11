namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result Index(CompilerObject @object, Argument[] arguments)
            => Result.Error(
                "Object does not support index operation."
            );

        public static Result Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
            => Result.Error(
                "Object does not support index assignment operation."
            );
    }
}
