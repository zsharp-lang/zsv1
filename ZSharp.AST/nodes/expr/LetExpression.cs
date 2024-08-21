namespace ZSharp.AST
{
    public sealed class LetExpression(string name) : Expression
    {
        public string Name { get; set; } = name;

        public Expression? Type { get; set; }

        public required Expression Value { get; set; }
    }
}
