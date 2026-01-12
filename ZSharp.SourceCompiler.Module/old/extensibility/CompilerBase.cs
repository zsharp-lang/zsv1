namespace ZSharp.ZSSourceCompiler
{
    public abstract class CompilerBase(ZSSourceCompiler compiler)
    {
        public ZSSourceCompiler Compiler { get; } = compiler;

        public Context Context => Compiler.Context;
    }
}
