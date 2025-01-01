namespace ZSharp.ZSSourceCompiler
{
    public sealed class NodeObject(Node node) : CompilerObject
    {
        public Node Node { get; } = node;
    }
}
