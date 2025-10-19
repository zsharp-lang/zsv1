namespace ZSharp.Compiler
{
    public interface ICTTypeMatch
    {
        public IResult<MatchResult, Error> Match(Compiler compiler, CompilerObject type);
    }
}
