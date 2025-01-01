using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        private readonly Mapping<CompilerObject, Node> nodes = [];

        public Node CurrentNode => Compiler<ContextCompiler>()?.ContextNode ?? throw new InvalidOperationException();

        public Node? Node(CompilerObject @object)
            => nodes.GetValueOrDefault(@object);

        public Node Node(CompilerObject @object, Node node)
            => nodes[@object] = node;
    }
}
