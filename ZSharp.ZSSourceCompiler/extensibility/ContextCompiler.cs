namespace ZSharp.ZSSourceCompiler
{
    public abstract class ContextCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
    {
        public abstract Node ContextNode { get; }

        public abstract CompilerObject ContextObject { get; }

        public abstract CompilerObject CompileNode();
    }

    public abstract class ContextCompiler<TNode, TObject>
        : ContextCompiler
        where TNode : Node
        where TObject : CompilerObject
    {
        public override Node ContextNode => Node;

        public override CompilerObject ContextObject => Object;

        public TNode Node { get; }

        public TObject Object { get; }

        public ContextCompiler(ZSSourceCompiler compiler, TNode node, TObject @object)
            : base(compiler)
        {
            Node = node;
            Object = @object;

            Context.Node(@object, node);
        }

        public sealed override CompilerObject CompileNode()
            => Compile();

        public virtual TObject Compile()
            => Object;
    }
}
