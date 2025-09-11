namespace ZSharp.Compiler
{
    public interface ICTTypeMatch
    {
        public Result<MatchResult> Match(Compiler compiler, CompilerObject type);
    }
}
