namespace ZSharp.AST
{
    public sealed class ArrayPattern : Pattern
    {
        public List<Pattern> Items { get; set; } = [];

        public Expression? ElementType { get; set; }
    }
}
