namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Whether a value matches a pattern, as in <c>value is let circle: Circle</c>. It
    /// stands for the matching REI's pattern match.
    /// </summary>
    public sealed class Match
        : Expression
    {
        public required Expression Value { get; set; }

        public required Pattern Pattern { get; set; }
    }
}
