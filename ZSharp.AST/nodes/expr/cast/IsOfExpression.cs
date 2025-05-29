namespace ZSharp.AST
{
    public sealed class IsOfExpression : Expression
    {
        public Expression Expression { get; set; }

        public string? Name { get; set; }

        public required Expression OfType { get; set; }
    }
}
