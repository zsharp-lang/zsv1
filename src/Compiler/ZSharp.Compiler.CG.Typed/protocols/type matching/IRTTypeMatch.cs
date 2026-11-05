namespace ZSharp.Compiler
{
    public interface IRTTypeMatch
    {
        public IResult<MatchResult, Error> Match(CompilerObject value, CompilerObject type);
    }
}
