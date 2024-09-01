namespace ZSharp.AST
{
    public sealed class CaseStatement : Statement
    {
        public Expression? Value { get; set; }

        public Expression? Of { get; set; }

        public List<WhenClauseStatement> WhenClauses { get; set; } = [];

        public Statement? Else { get; set; }
    }
}
