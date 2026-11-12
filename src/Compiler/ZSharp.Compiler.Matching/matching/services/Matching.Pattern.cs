namespace ZSharp.Compiler
{
    /// <summary>
    /// Whether a value matches a pattern, as in <c>value is Circle</c>: the condition, as a CO.
    /// </summary>
    public delegate IResult MatchPattern(CompilerObject value, CompilerObject pattern);

    partial class Matching
    {
        public MatchPattern MatchPattern { get; set; } = Dispatcher.Match;

        /// <summary>
        /// Whether a value matches a pattern.
        /// </summary>
        public IResult Match(CompilerObject value, CompilerObject pattern)
            => MatchPattern(value, pattern);
    }
}
