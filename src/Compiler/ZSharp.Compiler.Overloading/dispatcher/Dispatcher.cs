namespace ZSharp.Compiler
{
    internal static class Dispatcher
    {
        public static IResult Apply(CompilerObject[] candidates, Func<CompilerObject, IResult> operation)
            => Result.Error(new UnsupportedError("Nothing chooses between overloads."));

        public static bool IsMoreSpecific(CompilerObject candidate, CompilerObject other)
            => false;
    }
}
