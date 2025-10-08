using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ObjectType(TypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly TypeReference type = type;

        Result<IType> ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => Result<IType>.Ok(type);
    }
}
