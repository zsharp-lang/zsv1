namespace ZSharp.AST
{
    public sealed class WhileExpression : Expression
    {
        public required Expression Condition { get; set; }

        public string Name { get; set; } = string.Empty;

        public required Statement Body { get; set; }

        public Statement? Else { get; set; }
    }
}
