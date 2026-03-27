namespace ZSharp.AST
{
    public sealed partial class ObjectPattern : Pattern
    {
        public List<Item> Items { get; set; } = [];
    }
}
