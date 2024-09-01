namespace ZSharp.AST
{
    public sealed class Module : Expression
    {
        public List<Statement> Body { get; set; } = [];

        public required string Name { get; set; }
    }
}
