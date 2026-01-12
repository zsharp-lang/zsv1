using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class VoidType(TypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly TypeReference type = type;

        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => Result<IType>.Ok(type);
    }
}
