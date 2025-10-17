using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class RawIRCode(IRCode code)
        : CompilerObject
        , ICompileIRCode
    {
        private readonly IRCode code = code;

        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
            => Result<IRCode>.Ok(code);

        public static CompilerObject From(Collection<IR.VM.Instruction> code, CompilerObject type)
            => new UntypedIRCode(code, type);

        public static CompilerObject From(Collection<IR.VM.Instruction> code, IR.IType type)
            => From(new()
            {
                Instructions = code,
                Types = [type]
            });

        public static CompilerObject From(IRCode code)
            => new RawIRCode(code);
    }
}
