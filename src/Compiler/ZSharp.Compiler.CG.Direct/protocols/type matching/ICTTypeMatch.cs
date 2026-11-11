namespace ZSharp.Compiler
{
    public interface ICTTypeMatch
    {
        public IResult<MatchResult, Error> Match(CompilerObject type);
    }
}
