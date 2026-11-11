namespace ZSharp.AST
{
    public sealed class TypedNamePattern : Pattern
    {
        public required string Name { get; set; }

        public required Expression Type { get; set; }
    }
}
