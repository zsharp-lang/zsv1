namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result Call(CompilerObject callee, Argument[] arguments)
            => Result.Error(
                $"Object [{callee}] does not support call operation."
            );
    }
}
