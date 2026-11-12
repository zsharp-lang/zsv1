namespace ZSharp.Compiler
{
    internal static class Dispatcher
    {
        public static IResult Match(CompilerObject value, CompilerObject pattern)
            => Result.Error(new UnsupportedError($"Object [{value}] does not support pattern matching."));

        public static IResult Bind(CompilerObject parameter, ArgumentStream arguments)
            => Result.Error(new UnsupportedError($"Object [{parameter}] does not support matching arguments."));
    }
}
