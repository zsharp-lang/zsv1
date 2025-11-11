using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Global
        : ICTGet
    {
        IResult ICTGet.Get(Compiler.Compiler compiler)
            => Result.Ok(
                RawIRCode.From(
                    [ new IR.VM.GetGlobal(IR) ], Type
                )
            );
    }
}
