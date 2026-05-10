namespace ZSharp.Compiler
{
    public delegate IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern);

    partial class CG
    {
        public Match Match { get; set; } = Dispatcher.Match;
    }
}
