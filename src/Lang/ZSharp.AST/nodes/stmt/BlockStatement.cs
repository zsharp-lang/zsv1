namespace ZSharp.AST
{
    public sealed class BlockStatement : Statement
    {
        public List<Statement> Statements { get; set; } = [];
    }
}
