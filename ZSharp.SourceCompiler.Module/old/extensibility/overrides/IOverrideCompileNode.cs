namespace ZSharp.ZSSourceCompiler
{
    public interface IOverrideCompileNode<T>
        where T : Node
    {
        public ObjectResult? CompileNode(ZSSourceCompiler compiler, T node);
    }
}
