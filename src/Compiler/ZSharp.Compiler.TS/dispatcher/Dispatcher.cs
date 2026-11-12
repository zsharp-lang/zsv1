namespace ZSharp.Compiler
{
    internal static class Dispatcher
    {
        public static bool IsAssignableTo(CompilerObject target, CompilerObject source)
            => false;

        public static IResult TypeOf(CompilerObject @object)
            => Result.Error(
                new UnsupportedError($"Object [{@object}] does not support typeof.")
            );
    }
}
