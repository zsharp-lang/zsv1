namespace ZSharp.AST
{
    /// <summary>
    /// An expression as a pattern, matched with the match operator, as a type is in
    /// <c>value is Circle</c>.
    /// </summary>
    public sealed class ValuePattern : Pattern
    {
        public required Expression Value { get; set; }
    }
}
