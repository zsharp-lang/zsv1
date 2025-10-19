namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult Call(CompilerObject callee, Argument[] arguments)
            => Result.Error(
                $"Object [{callee}] does not support call operation."
            );
    }
}
