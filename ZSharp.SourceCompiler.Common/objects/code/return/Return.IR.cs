
namespace ZSharp.SourceCompiler
{
    partial class Return
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
        {
            var code = new IRCode();

            if (Value is not null)
                if (
                    compiler.IR.CompileCode(Value, target)
                    .When(out var valueCode)
                    .Error(out var error)
                ) return Result<IRCode>.Error(error);
                else code.Append(valueCode!);

            code.Instructions.Add(new IR.VM.Return());
            code.Types.Clear();

            return Result<IRCode>.Ok(code);
        }
    }
}
