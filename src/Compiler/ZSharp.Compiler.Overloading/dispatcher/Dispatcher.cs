namespace ZSharp.Compiler
{
    internal static class Dispatcher
    {
        public static IResult Apply(CompilerObject[] candidates, Func<CompilerObject, IResult> operation)
            => Result.Error(new UnsupportedError("Nothing chooses between overloads."));

        public static bool IsMoreSpecific(CompilerObject candidate, CompilerObject other)
            => false;

        public static IResult Group(CompilerObject[] candidates)
            => candidates is [var single]
                ? Result.Ok(single)
                : Result.Error(new UnsupportedError($"Nothing groups {candidates.Length} candidates as overloads."));
    }
}
