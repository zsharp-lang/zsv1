namespace ZSharp.AST
{
    public sealed class LetStatementDefinition : Node
    {
        public required string Name { get; set; }

        public Expression? Type { get; set; }

        public required Expression Value { get; set; }
    }

    public sealed class LetStatement : Statement
    {
        public List<LetStatementDefinition> Definitions { get; init; } = [];
    }
}
