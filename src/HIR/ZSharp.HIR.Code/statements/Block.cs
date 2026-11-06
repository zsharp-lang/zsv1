namespace ZSharp.HIR.Code
{
    public sealed class Block(params IEnumerable<Statement> statements)
        : Statement
    {
        public List<Local> Locals { get; } = [];

        public List<Statement> Statements { get; } = [.. statements];
    }
}
