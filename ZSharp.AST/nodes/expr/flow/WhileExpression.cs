namespace ZSharp.AST
{
    public sealed class WhileExpression<TElse> : Expression
        where TElse : Node
    {
        public required Expression Condition { get; set; }

        public string Name { get; set; } = string.Empty;

        public required Statement Body { get; set; }

        public TElse? Else { get; set; }
    }
}
