namespace ZSharp.AST
{
    public sealed class IfStatement : Statement
    {
        public required Expression Condition { get; set; }

        public required Statement If { get; set; }

        public Statement? Else { get; set; }
    }
}
