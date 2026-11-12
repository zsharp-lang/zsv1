namespace ZSharp.Compiler
{
    /// <summary>
    /// Whether a value matches a pattern, as in <c>value is of T</c>, where the pattern is
    /// a type: the condition, as a CO.
    /// </summary>
    public delegate IResult PatternMatch(CompilerObject value, CompilerObject pattern);

    partial class Matching
    {
        public PatternMatch Pattern { get; set; } = Dispatcher.Pattern;

        /// <summary>
        /// Whether a value matches a pattern.
        /// </summary>
        public IResult Match(CompilerObject value, CompilerObject pattern)
            => Pattern(value, pattern);
    }
}
