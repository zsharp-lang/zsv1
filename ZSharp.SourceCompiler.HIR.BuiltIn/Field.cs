namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public sealed class Field
        : Node
    {
        public required string Name { get; set; }

        public Node? Type { get; set; }

        public Node? Initializer { get; set; }
    }
}
