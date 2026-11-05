namespace ZSharp.AST
{
    public sealed class TuplePattern : Pattern
    {
        public List<Pattern> Items { get; } = [];
    }
}
