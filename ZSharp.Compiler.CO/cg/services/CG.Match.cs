namespace ZSharp.Compiler
{
    public delegate Result<MatchResult> Match(CompilerObject @object, CompilerObject pattern);

    partial struct CG
    {
        public Match Match { get; set; } = Dispatcher.Match;
    }
}
