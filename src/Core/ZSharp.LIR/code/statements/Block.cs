namespace ZSharp.LIR
{
    public sealed class Block(params IEnumerable<Statement> statements)
        : Statement
    {
        public List<Statement> Statements { get; } = [.. statements];
    }
}
