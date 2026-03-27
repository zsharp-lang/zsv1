namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class Parameter
        : Node
    {
        public required string Name { get; set; }

        public string? Alias { get; set; }

        public Node? Type { get; set; }

        public Node? Initializer { get; set; }
    }
}
