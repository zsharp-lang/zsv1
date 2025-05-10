namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        public ZSSourceCompiler SourceCompiler { get; }

        public Context(ZSSourceCompiler compiler, ScopeContext? globalScope = null)
        {
            SourceCompiler = compiler;

            GlobalScope = globalScope ?? new();
            compiler.Compiler.UseContext(CurrentScope = GlobalScope);

            compilerStack.Push(new DefaultContextCompiler(compiler));
        }
    }
}
