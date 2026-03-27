namespace ZSharp.SourceCompiler.HIR
{
    public sealed class VariableSpecification
        : Node
    {
        public required string Name { get; init; }

        public Node? Type { get; init; }

        public Node? Initializer { get; init; }
    }
}
