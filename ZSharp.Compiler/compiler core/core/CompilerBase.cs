namespace ZSharp.Compiler
{
    public abstract class CompilerBase(Compiler compiler)
    {
        public Compiler Compiler { get; } = compiler;

        public Context Context => Compiler.Context;
    }
}
