namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
            => Result<MatchResult>.Error(
                "Pattern matching is not supported in the current context."
            );
    }
}
