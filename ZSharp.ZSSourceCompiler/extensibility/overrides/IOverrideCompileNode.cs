namespace ZSharp.ZSSourceCompiler
{
    public interface IOverrideCompileNode<T>
        where T : Node
    {
        public CompilerObject? CompileNode(ZSSourceCompiler compiler, T node);
    }
}
