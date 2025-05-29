namespace ZSharp.AST
{
    public sealed class IsOfExpression : Expression
    {
        public required Expression Expression { get; set; }

        public string? Name { get; set; }

        public required Expression OfType { get; set; }
    }
}
