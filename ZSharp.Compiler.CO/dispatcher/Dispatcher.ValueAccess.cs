namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result Get(CompilerObject @object)
            => Result.Error(
                "Object does not support get"
            );

        public static Result Set(CompilerObject @object, CompilerObject value)
            => Result.Error(
                "Object does not support set"
            );
    }
}
