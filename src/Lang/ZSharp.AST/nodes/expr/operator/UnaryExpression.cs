namespace ZSharp.AST
{
    /// <summary>
    /// An operator written before its one operand, as <c>-x</c>.
    /// </summary>
    public sealed class UnaryExpression : Expression
    {
        public required string Operator { get; set; }

        public required Expression Operand { get; set; }
    }
}
