using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Global
        : ICTGet
    {
        Result ICTGet.Get(Compiler.Compiler compiler)
            => Result.Ok(new RawIRCode(new([
                new IR.VM.GetGlobal(IR)
            ])
            {
                Types = [ compiler.IR.CompileType(Type).Unwrap() ]
            }));
    }
}
