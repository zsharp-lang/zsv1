namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Whether a value matches a pattern, as in <c>value is of T</c>, where the pattern is
    /// a type. It stands for the matching REI's pattern match.
    /// </summary>
    public sealed class Match
        : Expression
    {
        public required Expression Value { get; set; }

        public required Expression Pattern { get; set; }
    }
}
