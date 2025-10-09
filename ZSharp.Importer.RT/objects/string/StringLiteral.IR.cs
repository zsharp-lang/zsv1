using ZSharp.Compiler;

namespace ZSharp.Importer.RT.Objects
{
    partial class StringLiteral
        : ICompileIRCode
    {
        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, TargetPlatform? target)
            => Result<IRCode>.Ok(new([
                new IR.VM.PutString(value)
            ])
            {
                Types = [type],
                MaxStackSize = 1
            });
    }
}
