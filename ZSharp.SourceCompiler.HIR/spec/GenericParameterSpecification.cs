namespace ZSharp.SourceCompiler.HIR
{
    public sealed class GenericParameterSpecification
        : Node
    {
        public required string Name { get; init; }

        public Node? Constraint { get; init; }
    }
}
