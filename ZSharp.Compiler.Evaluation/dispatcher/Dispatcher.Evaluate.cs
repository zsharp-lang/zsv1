namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult Evaluate(CompilerObject @object)
            => Result.Error(
                $"Object [{@object}] does not support evaluation."
            );
    }
}
