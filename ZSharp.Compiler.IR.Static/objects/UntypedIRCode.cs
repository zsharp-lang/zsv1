using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    internal sealed class UntypedIRCode(Collection<IR.VM.Instruction> code, CompilerObject type)
        : CompilerObject
        , ICompileIRCode
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
        {
            if (
                compiler.IR.CompileType(type)
                .When(out var irType)
                .Error(out var error)
            )
                return Result<IRCode>.Error(error!);

            return Result<IRCode>.Ok(new()
            {
                Instructions = code,
                Types = [irType!]
            });
        }
    }
}
