namespace ZSharp.AST
{
    public sealed class LetExpression : Definition
    {
        public required string Name { get; set; }

        public Expression? Type { get; set; }

        public required Expression Value { get; set; }
    }
}
