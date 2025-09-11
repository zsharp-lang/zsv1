namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result TypeOf(CompilerObject @object)
            => Result.Error(
                "Object does not support typeof"
            );
    }
}
