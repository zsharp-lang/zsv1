namespace ZSharp.CTRuntime
{
    internal abstract class CompilerBase(ZSCompiler compiler) 
    {
        public ZSCompiler Compiler { get; } = compiler;

        public CompilerContext Context => Compiler.Context;

        public DomainContext<CTType> CT => Context.CT;

        public DomainContext<RTType> RT => Context.RT;
    }
}
