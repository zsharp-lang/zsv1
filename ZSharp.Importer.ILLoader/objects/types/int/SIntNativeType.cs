using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class SIntNativeType(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        Result<IType> ICompileIRType.CompileIRType(Compiler.IR ir)
            => Result<IType>.Ok(type);
    }
}
