using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        private CompilerBase currentCompiler;

        public CompilerBase CurrentCompiler => currentCompiler;

        public DefaultContextCompiler DefaultCompiler { get; }

        public ContextManager Compiler(CompilerBase compiler)
        {
            (currentCompiler, compiler) = (compiler, currentCompiler);

            return new(() => currentCompiler = compiler);
        }
    }
}
