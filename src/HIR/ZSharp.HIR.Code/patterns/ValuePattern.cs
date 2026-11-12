namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A value as a pattern, matched with the match operator: a type is matched by being
    /// of it, as in <c>value is Circle</c>.
    /// </summary>
    public sealed class ValuePattern
        : Pattern
    {
        public required Expression Value { get; set; }
    }
}
