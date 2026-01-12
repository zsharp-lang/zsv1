namespace ZSharp.Compiler
{
    public interface IRTTypeMatch
    {
        public IResult<MatchResult, Error> Match(Compiler compiler, CompilerObject value, CompilerObject type);
    }
}
