using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class SInt32Type(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => Result<IType>.Ok(type);
    }
}
