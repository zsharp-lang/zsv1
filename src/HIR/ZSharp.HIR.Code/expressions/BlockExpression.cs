namespace ZSharp.HIR.Code
{
    public sealed class BlockExpression
        : Expression
    {
        public List<Local> Locals { get; } = [];

        public List<Statement> Statements { get; init; } = [];

        public required Expression Result { get; set; }
    }
}
