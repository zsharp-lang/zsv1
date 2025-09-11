namespace ZSharp.Compiler
{
    public interface IRTTypeMatch
    {
        public Result<MatchResult> Match(Compiler compiler, CompilerObject value, CompilerObject type);
    }
}
