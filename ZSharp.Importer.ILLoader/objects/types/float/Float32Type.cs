using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class Float32Type(OOPTypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly OOPTypeReference type = type;

        Result<IType> ICompileIRType.CompileIRType(Compiler.IR ir)
            => Result<IType>.Ok(type);
    }
}
