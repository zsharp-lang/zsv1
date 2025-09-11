namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result Do(CompilerObject @object)
            => Result.Error(
                "Object does not support do"
            );
    }
}
