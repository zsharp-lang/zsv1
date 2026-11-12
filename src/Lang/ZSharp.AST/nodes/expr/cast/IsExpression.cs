namespace ZSharp.AST
{
    /// <summary>
    /// <c>value is pattern</c>: whether the value matches the pattern, binding what it says.
    /// </summary>
    public sealed class IsExpression : Expression
    {
        public required Expression Expression { get; set; }

        public required Pattern Pattern { get; set; }
    }
}
