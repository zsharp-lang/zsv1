namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        public ZSSourceCompiler SourceCompiler { get; }

        public Context(ZSSourceCompiler compiler, Scope? globalScope = null)
        {
            SourceCompiler = compiler;

            GlobalScope = globalScope ?? new(null);
            CurrentScope = GlobalScope;

            compilerStack.Push(new DefaultContextCompiler(compiler));
        }
    }
}
