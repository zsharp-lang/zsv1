namespace ZSharp.SourceCompiler.HIR
{
    public sealed class ParameterSpecification
        : Node
    {
        public string Name { get; init; } = string.Empty;

        public string Alias { get; init; } = string.Empty;

        public Node? Type { get; init; }

        public Node? DefaultValue { get; init; }
    }
}
