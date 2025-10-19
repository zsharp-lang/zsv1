using ZSharp.Compiler;

namespace ZSharp.Importer.RT.Objects
{
    partial class StringLiteral
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, TargetPlatform? target)
            => Result<IRCode>.Ok(new([
                new IR.VM.PutString(value)
            ])
            {
                Types = [
                    compiler.IR.CompileType(type)
                    .Else(e => new ErrorMessage($"WTF string can't compile to IR type?? {e}"))
                    .Unwrap()
                ],
                MaxStackSize = 1
            });
    }
}
