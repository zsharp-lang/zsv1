using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    internal sealed class LateCompileIRCode(Func<Compiler.Compiler, IRCode> callback)
        : CompilerObject
        , ICompileIRCode
    {
        public IRCode CompileIRCode(Compiler.Compiler compiler)
            => callback(compiler);
    }
}
