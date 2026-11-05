namespace ZSharp.AST
{
    public sealed class WhenClauseStatement : Statement
    {
        public required Expression Value { get; set; }

        public Statement? Body { get; set; }
    }
}
