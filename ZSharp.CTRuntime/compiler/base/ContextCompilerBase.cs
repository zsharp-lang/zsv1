namespace ZSharp.CTRuntime
{
    internal abstract class ContextCompilerBase<TNode, TIR>(ZSCompiler compiler) : CompilerBase(compiler)
        where TNode : RAST.RNode
        where TIR : IR.IRObject
    {
        public TIR? IR { get; private set; }

        public TNode? Node { get; private set; }

        public void Compile(TNode node, TIR ir)
        {
            (node, Node) = (Node!, node);
            (ir, IR) = (IR!, ir);

            Compile();

            (Node, IR) = (node, ir);
        }

        protected abstract void Compile();
    }
}
