namespace ZSharp.ZSSourceCompiler
{
    public sealed class CompilationError(
        Node node,
        string error
    ) : Error
    {
        public Node Node { get; } = node;

        public string Error { get; } = error;
    }
}
