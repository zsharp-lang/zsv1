namespace ZSharp.Compiler
{
    public delegate IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern);

    partial struct CG
    {
        public Match Match { get; set; } = Dispatcher.Match;
    }
}
