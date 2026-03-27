using ZSharp.Compiler;

namespace ZSharp.Importer.RT.Objects
{
    partial class Literal
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, TargetPlatform? target)
            => Result<IRCode>.Ok(new([
                
            ])
            {
                Types = [type],
                MaxStackSize = 1
            });
    }
}
