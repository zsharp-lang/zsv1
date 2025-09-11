using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class RawIRCode(IRCode code)
        : CompilerObject
        , ICompileIRCode
    {
        private readonly IRCode code = code;

        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.IR ir, object? target)
            => Result<IRCode>.Ok(code);
    }
}
