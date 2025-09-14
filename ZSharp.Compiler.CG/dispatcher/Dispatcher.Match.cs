namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result<MatchResult> Match(CompilerObject @object, CompilerObject pattern)
            => Result<MatchResult>.Error(
                "Pattern matching is not supported in the current context."
            );
    }
}
