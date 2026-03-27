namespace ZSharp.SourceCompiler.HIR
{
    public sealed class DefinitionSpecification
        : Node
    {
        public string Name { get; init; } = string.Empty;

        public List<GenericParameterSpecification> GenericParameters { get; init; } = [];

        public List<Node> Bases { get; init; } = [];

        public List<Node> Members { get; init; } = [];
    }
}
