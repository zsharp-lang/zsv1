namespace ZSharp.SourceCompiler.HIR
{
    public sealed class FunctionSpecification
        : Node
    {
        public string Name { get; init; } = string.Empty;

        public List<GenericParameterSpecification> GenericParameters { get; init; } = [];

        public List<ParameterSpecification> Parameters { get; init; } = [];

        public Node? ReturnType { get; init; }

        public Node? Body { get; init; }
    }
}
